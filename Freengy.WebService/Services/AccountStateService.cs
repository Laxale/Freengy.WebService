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
using Freengy.WebService.Exceptions;
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

        private readonly UserInformerService informerService = UserInformerService.Instance;
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


        /// <summary>
        /// Initialize the service.
        /// </summary>
        public void Initialize() 
        {
            try
            {
                using (var context = new ComplexUserContext())
                {
                    foreach (ComplexUserAccount userAccount in context.Objects.ToList())
                    {
                        var state = new AccountStateModel
                        {
                            AccountModel = userAccount,
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
                    var complexAccountState = accountStates.First(state => state.ComplexAccount.Id == userId);
                    return complexAccountState;
                }
                catch (Exception ex)
                {
                    ex.Message.WriteToConsole();
                    return null;
                }
            }
        }

        public void EditAccountProps(Guid userId, EditAccountModel editRequest) 
        {
            lock (Locker)
            {
                try
                {
                    var targetAccState = accountStates.FirstOrDefault(state => state.ComplexAccount.Id == userId);

                    if (targetAccState == null) throw new InvalidOperationException($"Account '{ userId }' state not found in cache");

                    EditSimpleProperties(editRequest, targetAccState.ComplexAccount);
                    AccountDbInteracter.Instance.AddOrUpdate(targetAccState.ComplexAccount);

                    Task.Run(() => informerService.NotifyAllFriendsAboutUser(targetAccState));
                }
                catch (Exception ex)
                {
                    ex.Message.WriteToConsole(ConsoleColor.Red);
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
                requesterAccountState.OnlineStatus != AccountOnlineStatus.Offline &&
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
            ComplexAccountState accountState = GetStatusOf(userName);

            if (accountState == null)
            {
                throw new UserNotFoundException(userName);
            }

            accountState.Address = userAddress;
            accountState.ClientAuth.ClientToken = CreateNewToken();
            accountState.ClientAuth.ServerToken = CreateNewToken();
            accountState.ComplexAccount.LastLogInTime = DateTime.Now;
            accountState.OnlineStatus = AccountOnlineStatus.Online;

            AccountDbInteracter.Instance.AddOrUpdate(accountState.ComplexAccount);

            return accountState;
        }

        /// <summary>
        /// Log the user out.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>User account state model.</returns>
        public ComplexAccountState LogOut(string userName)
        {
            var accountState = accountStates.FirstOrDefault(state => state.ComplexAccount.Name == userName);

            if (accountState == null)
            {
                throw new InvalidOperationException($"Cannot log '{ userName }' out - not registered");
            }

            lock (Locker)
            {
                accountState.Address = string.Empty;
                accountState.OnlineStatus = AccountOnlineStatus.Offline;

                AccountDbInteracter.Instance.AddOrUpdate(accountState.ComplexAccount);

                return accountState;
            }
        }


        internal void RegisterNewUserState(ComplexUserAccount newUserAccount) 
        {
            CacheNewState(newUserAccount, string.Empty);
        }

        internal ComplexAccountState GetStatusOf(string userName) 
        {
            lock (Locker)
            {
                try
                {
                    var complexAccountState = accountStates.First(state => state.ComplexAccount.Name == userName);

                    return complexAccountState;
                }
                catch (Exception ex)
                {
                    ex.Message.WriteToConsole();
                    return null;
                }
            }
        }

        internal IEnumerable<ComplexAccountState> GetByNameFilter(string filter) 
        {
            return accountStates.Where(state => state.ComplexAccount.Name.ToLowerInvariant().Contains(filter));
        }

        internal IEnumerable<ComplexAccountState> GetAllOnline() 
        {
            return accountStates.Where(
                state => state.OnlineStatus == AccountOnlineStatus.Online ||
                         state.OnlineStatus == AccountOnlineStatus.Afk ||
                         state.OnlineStatus == AccountOnlineStatus.Busy ||
                         state.OnlineStatus == AccountOnlineStatus.DoNotDisturb);
        }


        private ComplexAccountState CacheNewState(UserAccountModel accountModel, string address) 
        {
            accountModel.LastLogInTime = DateTime.Now;

            var newState = new AccountStateModel
            {
                Address = address,
                AccountModel = accountModel,
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
                if (complexAccountState.OnlineStatus == AccountOnlineStatus.Offline) continue;

                if (complexAccountState.FailResponceCount > maxFailResponces)
                {
                    complexAccountState.FailResponceCount = 0;
                    complexAccountState.OnlineStatus = AccountOnlineStatus.Offline;
                    $"Considered user '{ complexAccountState.ComplexAccount.Name }' offline".WriteToConsole(ConsoleColor.Magenta);
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
                string address = $"{complexAccountState.Address}{Subroutes.NotifyClient.RequestUserState}";
                actor
                    .SetRequestAddress(address)
                    .AddHeader(FreengyHeaders.Server.ServerSessionTokenHeaderName, complexAccountState.ClientAuth.ServerToken);

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
                            responce = task.Result.Value;
                        }
                    })
                    .Wait();

                if (responce?.StatusCode != HttpStatusCode.OK)
                {
                    complexAccountState.FailResponceCount++;
                    string message = $"Client {complexAccountState.ComplexAccount.Name} replied wrong status '{ responce?.StatusCode }'. " +
                                     $"Count: {complexAccountState.FailResponceCount}";

                    message.WriteToConsole();
                }
            }
        }

        private static void EditSimpleProperties(EditAccountModel editRequest, UserAccountModel targetModel) 
        {
            targetModel.Name = editRequest.NewName;
        }
    }
}