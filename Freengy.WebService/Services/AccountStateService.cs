// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Freengy.Common.Enums;
using Freengy.Common.Models;
using Freengy.Common.Helpers;
using Freengy.WebService.Context;
using Freengy.WebService.Extensions;
using Freengy.WebService.Models;
using Freengy.WebService.Interfaces;

using NLog;


namespace Freengy.WebService.Services 
{
    internal class AccountStateService : IService 
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly object Locker = new object();

        private static AccountStateService instance;

        private readonly List<AccountStateModel> accountStates = new List<AccountStateModel>();


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
            try
            {
                using (var context = new ComplexUserContext())
                {
                    List<ComplexUserAccount> allUsers = context.Objects.ToList();

                    foreach (ComplexUserAccount account in allUsers)
                    {
                        var state = new AccountStateModel
                        {
                            Account = account,
                            Address = string.Empty,
                            SessionToken = string.Empty,
                            OnlineStatus = AccountOnlineStatus.Offline
                        };

                        accountStates.Add(state);
                    }
                }

                string message = $"Initialized {nameof(AccountStateService)}";
                Console.WriteLine(message);
                logger.Info(message);
            }
            catch (Exception ex)
            {
                string message = $"Failed to initialize {nameof(AccountStateService)}";
                Console.WriteLine(ex);
                logger.Error(ex, message);
            }
        }

        /// <summary>
        /// Get the state of a given account.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public AccountStateModel GetStatusOf(Guid userId) 
        {
            lock (Locker)
            {
                var stateModel = accountStates.FirstOrDefault(state => state.Account.Id == userId);

                return stateModel;
            }
        }

        public bool IsAuthorized(Guid requesterId, string requesterToken) 
        {
            AccountStateModel requesterAccountState = accountStates.FirstOrDefault(state => state.Account.Id == requesterId);

            bool isAuthorized =
                requesterAccountState != null && 
                requesterAccountState.OnlineStatus != AccountOnlineStatus.Offline && 
                requesterAccountState.SessionToken == requesterToken;

            return isAuthorized;
        }

        public AccountStateModel LogIn(string userName, string userAddress) 
        {
            ComplexUserAccount account = RegistrationService.Instance.FindByName(userName);

            if (account == null)
            {
                throw new InvalidOperationException($"Account '{ userName }' is not registered");
            }

            lock (Locker)
            {
                return LogInImpl(account, userAddress);
            }
        }

        public AccountStateModel LogOut(string userName) 
        {
            ComplexUserAccount account = RegistrationService.Instance.FindByName(userName);

            if (account == null)
            {
                throw new InvalidOperationException($"Cannot log '{ userName }' out - not registered");
            }

            lock (Locker)
            {
                return LogOutImpl(account);
            }
        }


        private AccountStateModel LogInImpl(UserAccountModel accountModel, string userAddress) 
        {
            AccountStateModel savedAccountState = accountStates.FirstOrDefault(state => state.Account.Id == accountModel.Id);

            if (savedAccountState == null)
            {
                var newModel = CreateNewState(accountModel, userAddress);
                newModel.Account.LastLogInTime = DateTime.Now;
                
                return newModel;
            }

            savedAccountState.Address = userAddress;
            savedAccountState.SessionToken = CreateSessionToken();
            savedAccountState.Account.LastLogInTime = DateTime.Now;
            savedAccountState.OnlineStatus = AccountOnlineStatus.Online;

            return savedAccountState;
        }

        private AccountStateModel LogOutImpl(UserAccountModel accountModel) 
        {
            AccountStateModel savedAccountState = accountStates.FirstOrDefault(state => state.Account.Id == accountModel.Id);

            if (savedAccountState == null)
            {
                throw new InvalidOperationException($"Cannot log '{ accountModel.Name }' out - account state not saved");
            }

            savedAccountState.Address = string.Empty;
            savedAccountState.OnlineStatus = AccountOnlineStatus.Offline;

            AccountDbInteracter.Instance.AddOrUpdate(savedAccountState.Account.ToComplex());

            return savedAccountState;
        }

        private AccountStateModel CreateNewState(UserAccountModel accountModel, string address) 
        {
            accountModel.LastLogInTime = DateTime.Now;

            var newState = new AccountStateModel
            {
                Address = address,
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