﻿// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Freengy.Common.Enums;
using Freengy.Common.Models;
using Freengy.WebService.Models;


namespace Freengy.WebService.Services 
{
    internal class AccountStateService 
    {
        private static readonly object Locker = new object();

        private static AccountStateService instance;

        private readonly List<AccountState> accountStates = new List<AccountState>();


        private AccountStateService() 
        {

        }


        /// <summary>
        /// Единственный инстанс <see cref="AccountStateService"/>.
        /// </summary>
        public static AccountStateService Instance 
        {
            get
            {
                lock (Locker)
                {
                    return instance ?? (instance = new AccountStateService());
                }
            }
        }

        public AccountOnlineStatus LogIn(Guid userId) 
        {
            UserAccount account = RegistrationService.Instance.FindById(userId);

            if (account == null)
            {
                return AccountOnlineStatus.DoesntExist;
            }

            return LogIn(account);
        }

        public AccountOnlineStatus LogIn(string userName) 
        {
            UserAccount account = RegistrationService.Instance.FindByName(userName);

            if (account == null)
            {
                return AccountOnlineStatus.DoesntExist;
            }

            return LogIn(account);
        }

        public AccountOnlineStatus LogIn(UserAccount account) 
        {
            if (account.UniqueId == Guid.Empty)
            {
                throw new InvalidOperationException("Account Id is empty");
            }

            lock (Locker)
            {
                AccountState loggedAccount = accountStates.FirstOrDefault(state => state.Account.Id == account.Id);

                if (loggedAccount == null)
                {
                    var newState = new AccountState(account)
                    {
                        LastLogInTime = DateTime.Now,
                        OnlineStatus = AccountOnlineStatus.Online
                    };

                    accountStates.Add(newState);

                    return AccountOnlineStatus.Online;
                }

                return loggedAccount.OnlineStatus;
            }
        }
    }
}