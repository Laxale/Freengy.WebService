// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Models;
using Freengy.Common.Restrictions;
using Freengy.Database.Context;
using Freengy.WebService.Context;
using Freengy.WebService.Extensions;
using Freengy.WebService.Helpers;
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


        private RegistrationService() { }


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
                    //var targetAcc = registeredAccounts.FirstOrDefault(acc => acc.UniqueId == account.UniqueId);
                    var targetAcc = registeredAccounts.FirstOrDefault(acc => acc.Id == account.Id);

                    if(targetAcc == null) throw new InvalidOperationException($"Account '{ account.Id }' not found in cache");

                    AccountDbInteracter.TransferProperties(account, targetAcc);
                }
                catch (Exception ex)
                {
                    ex.Message.WriteToConsole(ConsoleColor.Red);
                }
            }
        }

        public bool ValidatePassword(string userName, string saltedPasswordHash) 
        {
            var targetAcc = registeredAccounts.FirstOrDefault(acc => acc.Name == userName);

            if (targetAcc == null)
            {
                throw new InvalidOperationException($"Account '{ userName }' is not registered");
            }

            var hasher = new Hasher();
            var doubleHash = hasher.GetHash(targetAcc.PasswordData.NextLoginSalt + saltedPasswordHash);

            bool areEqual = doubleHash == targetAcc.PasswordData.NextPasswordHash;

            return areEqual;
        }

        public RegistrationStatus RegisterAccount(RegistrationRequest request, out ComplexUserAccount registeredAcc) 
        {
            UserAccountModel existingAccount = FindByName(request.UserName);

            if (existingAccount != null)
            {
                registeredAcc = null;
                return RegistrationStatus.AlreadyExists;
            }

            var newAccount = new ComplexUserAccount
            {
                Name = request.UserName
            };

            Password passwordData = CreatePasswordData(request.Password);
            passwordData.ParentId = newAccount.Id;

            newAccount.PasswordDatas.Add(passwordData);

            RegistrationStatus result = RegisterAccount(newAccount, out newAccount);

            registeredAcc = newAccount;
            registeredAcc.PasswordData = registeredAcc.PasswordDatas.First();

            return result;
        }

        public ComplexUserAccount FindById(Guid userId) 
        {
            lock (Locker)
            {
                //ComplexUserAccount foundUser = registeredAccounts.FirstOrDefault(acc => acc.UniqueId == userId);
                ComplexUserAccount foundUser = registeredAccounts.FirstOrDefault(acc => acc.Id == userId);

                return foundUser;
            }
        }

        public ComplexUserAccount FindByName(string userName) 
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));

            lock (Locker)
            {
                ComplexUserAccount foundUser = registeredAccounts.FirstOrDefault(acc => acc.Name == userName);

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
                    selector = acc => true;
                }
                else
                {
                    selector = acc =>
                    {
                        if (acc.Name.Contains(nameFilter))
                        {
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
                        .Include(acc => acc.PasswordDatas)
                        .Include(acc => acc.Friendships)
                        .Include(acc => acc.FriendRequests)
                        .ToList();

                if (!objects.Any()) return;

                foreach (ComplexUserAccount account in objects)
                {
                    var realAccount = (ComplexUserAccount)account.CreateFromProxy(account);
                    realAccount.PasswordData = realAccount.PasswordDatas[0];

                    // автоматически из контекста цепляются только дружбы по основному внешнему ключу - исходящие.
                    // руками нужно добавить дружбы, которые для данного аккаунта являются входящими
                    var incomingFriendships = FriendshipService.Instance.FindByAcceptor(realAccount.Id);
                    realAccount.Friendships.AddRange(incomingFriendships);

                    registeredAccounts.Add(realAccount);
                }
            }
        }

        private static Password CreatePasswordData(string password) 
        {
            var hasher = new Hasher();
            string nextSalt = hasher.GetHash(Guid.NewGuid().ToString());
            string passwordHash = hasher.GetHash(nextSalt + password);
            string nextTotalHash = hasher.GetHash(nextSalt + passwordHash);

            var passwordData = new Password
            {
                NextLoginSalt = nextSalt,
                NextPasswordHash = nextTotalHash
            };

            return passwordData;
        }

        private RegistrationStatus RegisterAccount(ComplexUserAccount newAccount, out ComplexUserAccount registeredAcc) 
        {
            lock (Locker)
            {
                registeredAcc = null;

                try
                {
                    UserAccountModel trimmedAccount = new AccountValidator(newAccount).Trim();
                    ComplexUserAccount trimmedComplexAcc = trimmedAccount.ToComplex();
                    trimmedComplexAcc.PasswordDatas.AddRange(newAccount.PasswordDatas);

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
                    message.WriteToConsole(ConsoleColor.Red);

                    return RegistrationStatus.Error;
                }
            }
        }
    }
}