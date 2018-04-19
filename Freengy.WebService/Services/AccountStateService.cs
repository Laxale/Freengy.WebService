// Created by Laxale 17.04.2018
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


        public AccountOnlineStatus LogIn(string userName, out AccountState loggedAccountState) 
        {
            ComplexUserAccount account = RegistrationService.Instance.FindByName(userName);

            if (account == null)
            {
                loggedAccountState = null;
                return AccountOnlineStatus.DoesntExist;
            }

            AccountOnlineStatus result = LogIn(account, out AccountState state);
            loggedAccountState = state;

            return result;
        }


        private AccountOnlineStatus LogIn(UserAccount account, out AccountState loggedAccountState) 
        {
            if (account.UniqueId == Guid.Empty)
            {
                throw new InvalidOperationException("Account Id is empty");
            }

            lock (Locker)
            {
                AccountState accountState = accountStates.FirstOrDefault(state => state.Account.Id == account.Id);

                if (accountState == null)
                {
                    account.LastLogInTime = DateTime.Now;

                    var newState = new AccountState
                    {
                        Account = account,
                        OnlineStatus = AccountOnlineStatus.Online
                    };

                    accountStates.Add(newState);
                    loggedAccountState = newState;

                    return AccountOnlineStatus.Online;
                }

                accountState.Account.LastLogInTime = DateTime.Now;
                loggedAccountState = accountState;

                AccountDbInteracter.Instance.AddOrUpdate((ComplexUserAccount)accountState.Account);

                return accountState.OnlineStatus;
            }
        }
    }
}