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

        private readonly Dictionary<AccountStateModel, SessionAuth> accountStates = new Dictionary<AccountStateModel, SessionAuth>();
        

        private AccountStateService() { }


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
                            OnlineStatus = AccountOnlineStatus.Offline
                        };

                        accountStates.Add(state, new SessionAuth());
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
        /// <param name="userId">Account identifier.</param>
        /// <returns>Account state model.</returns>
        public AccountStateModel GetStatusOf(Guid userId) 
        {
            lock (Locker)
            {
                try
                {
                    var stateModel = accountStates.First(statePair => statePair.Key.Account.Id == userId);
                    return stateModel.Key;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// Check if user is authorized.
        /// </summary>
        /// <param name="requesterId">Target use identifier.</param>
        /// <param name="requesterToken">Target user session token.</param>
        /// <returns>True if user is authorized.</returns>
        public bool IsAuthorized(Guid requesterId, string requesterToken) 
        {
            AccountStateModel requesterAccountState = GetStatusOf(requesterId);

            bool isAuthorized =
                requesterAccountState != null &&
                requesterAccountState.OnlineStatus != AccountOnlineStatus.Offline &&
                accountStates[requesterAccountState].ClientToken == requesterToken;

            return isAuthorized;
        }

        public KeyValuePair<AccountStateModel, SessionAuth> LogIn(string userName, string userAddress) 
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


        private KeyValuePair<AccountStateModel, SessionAuth> LogInImpl(UserAccountModel accountModel, string userAddress) 
        {
            AccountStateModel savedAccountState = GetStatusOf(accountModel.Id);

            if (savedAccountState == null)
            {
                KeyValuePair<AccountStateModel, SessionAuth> statePair = CreateNewStatePair(accountModel, userAddress);

                return statePair;
            }

            savedAccountState.Address = userAddress;
            savedAccountState.Account.LastLogInTime = DateTime.Now;
            savedAccountState.OnlineStatus = AccountOnlineStatus.Online;

            accountStates[savedAccountState].ClientToken = CreateNewToken();
            accountStates[savedAccountState].ServerToken = CreateNewToken();

            return accountStates.First(pair => pair.Key.Account.Id == savedAccountState.Account.Id);
        }

        private AccountStateModel LogOutImpl(UserAccountModel accountModel)
        {
            AccountStateModel savedAccountState = GetStatusOf(accountModel.Id);

            if (savedAccountState == null)
            {
                throw new InvalidOperationException($"Cannot log '{ accountModel.Name }' out - account state not saved");
            }

            savedAccountState.Address = string.Empty;
            savedAccountState.OnlineStatus = AccountOnlineStatus.Offline;

            AccountDbInteracter.Instance.AddOrUpdate(savedAccountState.Account.ToComplex());

            return savedAccountState;
        }

        private KeyValuePair<AccountStateModel, SessionAuth> CreateNewStatePair(UserAccountModel accountModel, string address) 
        {
            accountModel.LastLogInTime = DateTime.Now;

            var newState = new AccountStateModel
            {
                Address = address,
                Account = accountModel,
                OnlineStatus = AccountOnlineStatus.Online
            };
            var auth = new SessionAuth
            {
                ClientToken = CreateNewToken(),
                ServerToken = CreateNewToken()
            };

            var pair = new KeyValuePair<AccountStateModel, SessionAuth>(newState, auth);
            accountStates.Add(newState, auth);

            return pair;
        }

        private string CreateNewToken() 
        {
            string source = Guid.NewGuid().ToString();

            string token = new Hasher().GetHash(source);

            return token;
        }
    }
}