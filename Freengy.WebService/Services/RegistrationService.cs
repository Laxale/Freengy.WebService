// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Freengy.Common.Enums;
using Freengy.Common.Models;
using Freengy.Common.Restrictions;
using Freengy.Database.Context;

using NLog;


namespace Freengy.WebService.Services 
{
    /// <summary>
    /// Service to manage user account registrations.
    /// </summary>
    internal class RegistrationService 
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly object Locker = new object();

        private static RegistrationService instance;

        private readonly List<UserAccount> registeredAccounts = new List<UserAccount>();


        private RegistrationService() 
        {
            ReadAccounts();
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


        public RegistrationStatus RegisterAccount(string userName, out UserAccount registeredAcc) 
        {
            var newAccount = new UserAccount
            {
                Name = userName
            };

            newAccount.UniqueId = Guid.Parse(newAccount.Id);

            RegistrationStatus result = RegisterAccount(newAccount, out newAccount);

            registeredAcc = newAccount;

            return result;
        }


        private RegistrationStatus RegisterAccount(UserAccount newAccount, out UserAccount registeredAcc) 
        {
            lock (Locker)
            {
                registeredAcc = null;

                try
                {
                    UserAccount existingAccount = FindByName(newAccount.Name);

                    if (existingAccount != null)
                    {
                        return RegistrationStatus.AlreadyExists;
                    }

                    UserAccount trimmedAccount = new AccountValidator(newAccount).Trim();

                    trimmedAccount.RegistrationTime = DateTime.Now;

                    AccountDbInteracter.Instance.AddOrUpdate(trimmedAccount);

                    registeredAccounts.Add(trimmedAccount);

                    registeredAcc = trimmedAccount;

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

        public UserAccount FindById(Guid userId) 
        {
            lock (Locker)
            {
                UserAccount foundUser = registeredAccounts.FirstOrDefault(acc => acc.UniqueId == userId);

                return foundUser;
            }
        }

        public UserAccount FindByName(string userName) 
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));

            lock (Locker)
            {
                UserAccount foundUser = registeredAccounts.FirstOrDefault(acc => acc.Name == userName);

                foundUser?.SyncUniqueIdToId();

                return foundUser;
            }
        }


        private void ReadAccounts() 
        {
            using (var dbContext = new SimpleDbContext<UserAccount>())
            {
                foreach (UserAccount account in dbContext.Objects)
                {
                    registeredAccounts.Add(account);
                }
            }
        }
    }
}