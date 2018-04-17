// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Freengy.WebService.Enums;
using Freengy.WebService.Models;

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


        public RegistrationStatus RegisterAccount(string userName) 
        {
            var newAccount = new UserAccount
            {
                Id = Guid.NewGuid(),
                Name = userName
            };

            return RegisterAccount(newAccount);
        }

        public RegistrationStatus RegisterAccount(UserAccount newAccount) 
        {
            lock (Locker)
            {
                try
                {
                    UserAccount existingAccount = FindById(newAccount.Id);

                    if (existingAccount == null)
                    {
                        registeredAccounts.Add(newAccount);

                        return RegistrationStatus.Registered;
                    }

                    return RegistrationStatus.AlreadyExists;
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
                UserAccount foundUser = registeredAccounts.FirstOrDefault(acc => acc.Id == userId);

                return foundUser;
            }
        }
    }
}