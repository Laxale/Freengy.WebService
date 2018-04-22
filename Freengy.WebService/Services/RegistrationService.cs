// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

using Freengy.Common.Enums;
using Freengy.Common.Models;
using Freengy.Common.Restrictions;
using Freengy.Database.Context;
using Freengy.WebService.Context;
using Freengy.WebService.Extensions;
using Freengy.WebService.Interfaces;
using Freengy.WebService.Models;

using NLog;


namespace Freengy.WebService.Services 
{
    /// <summary>
    /// Service to manage user account registrations.
    /// </summary>
    internal class RegistrationService : IService 
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly object Locker = new object();

        private static RegistrationService instance;

        private readonly List<ComplexUserAccount> registeredAccounts = new List<ComplexUserAccount>();


        private RegistrationService() 
        {
            
        }


        /// <summary>
        /// Единственный инстанс <see cref="RegistrationService"/>.
        /// </summary>
        public static RegistrationService Instance 
        {
            get
            {
                lock (Locker)
                {
                    return instance ?? (instance = new RegistrationService());
                }
            }
        }


        /// <inheritdoc />
        /// <summary>
        /// Initialize the service.
        /// </summary>
        public void Initialize() 
        {
            ReadAccounts();
        }

        public void UpdateCache(ComplexUserAccount account) 
        {
            lock (Locker)
            {
                try
                {
                    var targetAcc = registeredAccounts.FirstOrDefault(acc => acc.UniqueId == account.UniqueId);

                    if(targetAcc == null) throw new InvalidOperationException($"Account '{ account.UniqueId }' not found in cache");

                    AccountDbInteracter.TransferProperties(account, targetAcc);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public RegistrationStatus RegisterAccount(string userName, out ComplexUserAccount registeredAcc) 
        {
            var newAccount = new ComplexUserAccount
            {
                Name = userName
            };

            newAccount.UniqueId = Guid.Parse(newAccount.Id);

            RegistrationStatus result = RegisterAccount(newAccount, out newAccount);

            registeredAcc = newAccount;

            return result;
        }
        
        public ComplexUserAccount FindById(Guid userId) 
        {
            lock (Locker)
            {
                ComplexUserAccount foundUser = registeredAccounts.FirstOrDefault(acc => acc.UniqueId == userId);

                return foundUser;
            }
        }

        public ComplexUserAccount FindByName(string userName) 
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));

            lock (Locker)
            {
                ComplexUserAccount foundUser = registeredAccounts.FirstOrDefault(acc => acc.Name == userName);

                foundUser?.SyncUniqueIdToId();

                return foundUser;
            }
        }

        public IEnumerable<ComplexUserAccount> FindByNameFilter(string nameFilter) 
        {
            if (string.IsNullOrWhiteSpace(nameFilter)) return new List<ComplexUserAccount>();

            lock (Locker)
            {
                Func<ComplexUserAccount, bool> selector;
                if(string.IsNullOrWhiteSpace(nameFilter))
                {
                    selector = acc =>
                    {
                        acc.SyncUniqueIdToId();
                        return true;
                    };
                }
                else
                {
                    selector = acc =>
                    {
                        if (acc.Name.Contains(nameFilter))
                        {
                            acc.SyncUniqueIdToId();
                            return true;
                        }

                        return false;
                    };
                }

                IEnumerable<ComplexUserAccount> foundUsers = registeredAccounts.Where(selector);
                
                return foundUsers;
            }
        }


        private void ReadAccounts() 
        {
            using (var dbContext = new ComplexUserContext())
            {
                var objects = 
                    dbContext
                        .Objects
                        .Include(acc => acc.Friendships)
                        .Include(acc => acc.FriendRequests);

                if (!objects.Any()) return;

                foreach (ComplexUserAccount account in objects)
                {
                    var realAccount = (ComplexUserAccount)account.CreateFromProxy(account);

                    realAccount.SyncUniqueIdToId();
                    registeredAccounts.Add(account);
                }
            }
        }

        private RegistrationStatus RegisterAccount(ComplexUserAccount newAccount, out ComplexUserAccount registeredAcc) 
        {
            lock (Locker)
            {
                registeredAcc = null;

                try
                {
                    UserAccountModel existingAccount = FindByName(newAccount.Name);

                    if (existingAccount != null)
                    {
                        return RegistrationStatus.AlreadyExists;
                    }

                    UserAccountModel trimmedAccount = new AccountValidator(newAccount).Trim();
                    ComplexUserAccount trimmedComplexAcc = trimmedAccount.ToComplex();

                    trimmedComplexAcc.RegistrationTime = DateTime.Now;

                    AccountDbInteracter.Instance.AddOrUpdate(trimmedComplexAcc);

                    registeredAccounts.Add(trimmedComplexAcc);

                    registeredAcc = trimmedComplexAcc;

                    return RegistrationStatus.Registered;

                }
                catch (Exception ex)
                {
                    string message = $"Failed to register account '{newAccount.Name}': {ex.Message}";
                    logger.Error(ex, "Failed to register account");
                    Console.WriteLine(message);

                    return RegistrationStatus.Error;
                }
            }
        }
    }
}