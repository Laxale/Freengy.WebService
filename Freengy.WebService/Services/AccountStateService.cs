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
using System.Net;
using System.Net.Http;
using System.Threading;

using Freengy.Common.Enums;
using Freengy.Common.Models;
using Freengy.Common.Helpers;
using Freengy.Common.Constants;
using Freengy.Common.Extensions;
using Freengy.WebService.Context;
using Freengy.WebService.Extensions;
using Freengy.WebService.Helpers;
using Freengy.WebService.Models;
using Freengy.WebService.Interfaces;

using NLog;


namespace Freengy.WebService.Services 
{
    internal class AccountStateService : IService 
    {
        private static readonly int maxFailResponces = 3;
        private static readonly int updatePeriodInMs = 1000;
        private static readonly object Locker = new object();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static AccountStateService instance;

        private readonly List<ComplexAccountState> accountStates = new List<ComplexAccountState>();
        

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
                foreach (ComplexUserAccount userAccount in RegistrationService.Instance.GetAllForInitialize())
                {
                    var state = new AccountStateModel
                    {
                        Account = userAccount,
                        Address = string.Empty,
                        OnlineStatus = AccountOnlineStatus.Offline
                    };

                    accountStates.Add(new ComplexAccountState(state));
                }

                StartUpdateCycle();

                string message = $"Initialized {nameof(AccountStateService)}";
                //message.WriteToConsole();
                logger.Info(message);
            }
            catch (Exception ex)
            {
                string message = $"Failed to initialize {nameof(AccountStateService)}";
                message.WriteToConsole();
                logger.Error(ex, message);
            }
        }

        /// <summary>
        /// Get the state of a given account.
        /// </summary>
        /// <param name="userId">Account identifier.</param>
        /// <returns>Account state model.</returns>
        public ComplexAccountState GetStatusOf(Guid userId) 
        {
            lock (Locker)
            {
                try
                {
                    var complexAccountState = accountStates.First(statePair => statePair.StateModel.Account.Id == userId);
                    return complexAccountState;
                }
                catch (Exception ex)
                {
                    ex.Message.WriteToConsole();
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
            ComplexAccountState requesterAccountState = GetStatusOf(requesterId);

            bool isAuthorized =
                requesterAccountState != null &&
                requesterAccountState.StateModel.OnlineStatus != AccountOnlineStatus.Offline &&
                requesterAccountState.ClientAuth.ClientToken == requesterToken;

            return isAuthorized;
        }

        /// <summary>
        /// Log the user in.
        /// </summary>
        /// <param name="userName">User account name.</param>
        /// <param name="userAddress">User address.</param>
        /// <returns>Account state model and user authentication.</returns>
        public ComplexAccountState LogIn(string userName, string userAddress) 
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

        /// <summary>
        /// Log the user out.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>User account state model.</returns>
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


        private ComplexAccountState LogInImpl(UserAccountModel accountModel, string userAddress) 
        {
            ComplexAccountState savedAccountState = GetStatusOf(accountModel.Id);

            if (savedAccountState == null)
            {
                ComplexAccountState statePair = CreateNewStatePair(accountModel, userAddress);

                return statePair;
            }

            savedAccountState.StateModel.Address = userAddress;
            savedAccountState.ClientAuth.ClientToken = CreateNewToken();
            savedAccountState.ClientAuth.ServerToken = CreateNewToken();
            savedAccountState.StateModel.Account.LastLogInTime = DateTime.Now;
            savedAccountState.StateModel.OnlineStatus = AccountOnlineStatus.Online;

            return savedAccountState;
        }

        private AccountStateModel LogOutImpl(UserAccountModel accountModel) 
        {
            ComplexAccountState savedAccountState = GetStatusOf(accountModel.Id);

            if (savedAccountState == null)
            {
                throw new InvalidOperationException($"Cannot log '{ accountModel.Name }' out - account state not saved");
            }

            savedAccountState.StateModel.Address = string.Empty;
            savedAccountState.StateModel.OnlineStatus = AccountOnlineStatus.Offline;

            AccountDbInteracter.Instance.AddOrUpdate(savedAccountState.StateModel.Account.ToComplex());

            return savedAccountState.StateModel;
        }

        private ComplexAccountState CreateNewStatePair(UserAccountModel accountModel, string address) 
        {
            accountModel.LastLogInTime = DateTime.Now;

            var newState = new AccountStateModel
            {
                Address = address,
                Account = accountModel,
                OnlineStatus = AccountOnlineStatus.Online
            };
            
            var complexState = new ComplexAccountState(newState);
            complexState.ClientAuth.ClientToken = CreateNewToken();
            complexState.ClientAuth.ServerToken = CreateNewToken();
            accountStates.Add(complexState);

            return complexState;
        }

        private string CreateNewToken() 
        {
            string source = Guid.NewGuid().ToString();

            string token = new Hasher().GetHash(source);

            return token;
        }

        private void StartUpdateCycle() 
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(updatePeriodInMs);

                    Update();
                }
            });
        }

        private void Update() 
        {
            foreach (ComplexAccountState complexAccountState in accountStates)
            {
                if (complexAccountState.StateModel.OnlineStatus == AccountOnlineStatus.Offline) continue;

                if (complexAccountState.FailResponceCount > maxFailResponces)
                {
                    complexAccountState.FailResponceCount = 0;
                    complexAccountState.StateModel.OnlineStatus = AccountOnlineStatus.Offline;
                    $"Considered user '{ complexAccountState.StateModel.Account.Name }' offline".WriteToConsole(ConsoleColor.Magenta);
                    continue;
                }

                try
                {
                    GetClientState(complexAccountState);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                    complexAccountState.FailResponceCount++;
                }
            }
        }

        private static void GetClientState(ComplexAccountState complexAccountState) 
        {
            using (var actor = new HttpActor())
            {
                string address = $"{complexAccountState.StateModel.Address}{Subroutes.NotifyClient.RequestUserState}";
                actor
                    .SetRequestAddress(address)
                    .AddHeader(FreengyHeaders.ServerSessionTokenHeaderName, complexAccountState.ClientAuth.ServerToken);

                HttpResponseMessage responce = null;
                actor
                    .GetAsync()
                    .ContinueWith(task =>
                    {
                        if (task.Exception != null)
                        {
                            responce = null;
                            complexAccountState.FailResponceCount++;
                            logger.Error(task.Exception.GetReallyRootException(), "Failed to get async response");
                        }
                        else
                        {
                            responce = task.Result;
                        }
                    })
                    .Wait();

                if (responce?.StatusCode != HttpStatusCode.OK)
                {
                    complexAccountState.FailResponceCount++;
                    string message = $"Client {complexAccountState.StateModel.Account.Name} replied wrong status '{ responce?.StatusCode }'. " +
                                     $"Count: {complexAccountState.FailResponceCount}";

                    message.WriteToConsole();
                }
            }
        }
    }
}