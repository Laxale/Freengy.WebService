// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;

using Freengy.Common.Enums;
using Freengy.Common.Models;
using Freengy.Common.Helpers;
using Freengy.WebService.Models;
using Freengy.WebService.Interfaces;


namespace Freengy.WebService.Services 
{
    internal class AccountStateService : IService 
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


        /// <inheritdoc />
        /// <summary>
        /// Initialize the service.
        /// </summary>
        public void Initialize() 
        {
            
        }

        public bool IsAuthorized(Guid requesterId, string requesterToken) 
        {
            //AccountState requesterAccountState = accountStates.FirstOrDefault(state => state.Account.UniqueId == requesterId);
            AccountState requesterAccountState = accountStates.FirstOrDefault(state => state.Account.Id == requesterId);

            bool isAuthorized =
                requesterAccountState != null && 
                requesterAccountState.OnlineStatus != AccountOnlineStatus.Offline && 
                requesterAccountState.SessionToken == requesterToken;

            return isAuthorized;
        }

        public AccountOnlineStatus LogIn(string userName, out AccountState loggedAccountState) 
        {
            return LogInOrOut(userName, true, out loggedAccountState);
        }

        public AccountOnlineStatus LogOut(string userName, out AccountState loggedAccountState) 
        {
            return LogInOrOut(userName, false, out loggedAccountState);
        }


        private AccountOnlineStatus LogInOrOut(string userName, bool isLoggingIn, out AccountState loggedAccountState) 
        {
            ComplexUserAccount account = RegistrationService.Instance.FindByName(userName);

            if (account == null)
            {
                loggedAccountState = null;
                return AccountOnlineStatus.DoesntExist;
            }

            AccountOnlineStatus result = InvokeLogProcess(account, isLoggingIn, out AccountState state);
            loggedAccountState = state;

            return result;
        }


        private AccountOnlineStatus InvokeLogProcess(UserAccountModel accountModel, bool isIn, out AccountState loggedAccountState) 
        {
            //if (accountModel.UniqueId == Guid.Empty)
            if (accountModel.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Account Id is empty");
            }

            lock (Locker)
            {
                return InvokeImpl(accountModel, isIn, out loggedAccountState);
            }
        }

        private AccountOnlineStatus InvokeImpl(UserAccountModel accountModel, bool isIn, out AccountState loggedAccountState) 
        {
            AccountState accountState = accountStates.FirstOrDefault(state => state.Account.Id == accountModel.Id);

            if (accountState == null)
            {
                if (!isIn)
                {
                    throw new InvalidOperationException($"Tried to log out '{accountModel.Name}', who is not present");
                }

                loggedAccountState = CreateNewState(accountModel);

                return AccountOnlineStatus.Online;
            }

            if (isIn)
            {
                accountState.Account.LastLogInTime = DateTime.Now;
                accountState.OnlineStatus = AccountOnlineStatus.Online;
            }
            else
            {
                //TODO: disconnect user from all activities
                accountState.OnlineStatus = AccountOnlineStatus.Offline;
            }

            loggedAccountState = accountState;

            AccountDbInteracter.Instance.AddOrUpdate((ComplexUserAccount) accountState.Account);

            return accountState.OnlineStatus;
        }

        private AccountState CreateNewState(UserAccountModel accountModel) 
        {
            accountModel.LastLogInTime = DateTime.Now;

            var newState = new AccountState
            {
                Account = accountModel,
                SessionToken = CreateSessionToken(),
                OnlineStatus = AccountOnlineStatus.Online
            };

            accountStates.Add(newState);
            return newState;
        }

        private string CreateSessionToken() 
        {
            string source = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

            string token = new Hasher().GetHash(source);

            return token;
        }
    }
}