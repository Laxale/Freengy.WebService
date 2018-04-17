// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Freengy.WebService.Models;


namespace Freengy.WebService.Services 
{
    /// <summary>
    /// Service to manage user account registrations.
    /// </summary>
    internal class UserAccountService 
    {
        private static readonly object Locker = new object();

        private static UserAccountService instance;

        private readonly ObservableCollection<UserAccount> registeredAccounts = new ObservableCollection<UserAccount>();


        private UserAccountService() 
        {

        }


        /// <summary>
        /// Единственный инстанс <see cref="UserAccountService"/>.
        /// </summary>
        public static UserAccountService Instance 
        {
            get
            {
                lock (Locker)
                {
                    return instance ?? (instance = new UserAccountService());
                }
            }
        }


        public bool RegisterAccount(string userName) 
        {
            var newAccount = new UserAccount
            {
                Id = Guid.NewGuid(),
                Name = userName
            };

            return RegisterAccount(newAccount);
        }

        public bool RegisterAccount(UserAccount newAccount) 
        {
            lock (Locker)
            {
                UserAccount existingAccount = FindById(newAccount.Id);

                if (existingAccount == null)
                {
                    registeredAccounts.Add(newAccount);

                    return true;
                }

                return false;
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