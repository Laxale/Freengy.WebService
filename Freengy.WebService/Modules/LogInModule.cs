// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Freengy.Common.Constants;
using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Interfaces;
using Freengy.Common.Models;
using Freengy.Common.Models.Readonly;
using Freengy.WebService.Helpers;
using Freengy.WebService.Services;

using Nancy;

using Newtonsoft.Json;
using NLog.Targets.Wrappers;


namespace Freengy.WebService.Modules 
{
    using Freengy.WebService.Models;

    using Nancy.Responses;


    /// <summary>
    /// Module for user log in action.
    /// </summary>
    public class LogInModule : NancyModule 
    {
        public LogInModule() 
        {
           $"Created { nameof(LogInModule) }".WriteToConsole();

            Post[Subroutes.Login] = OnLoginRequest;
        }


        private dynamic OnLoginRequest(dynamic arg) 
        {
            var stateService = AccountStateService.Instance;
            LoginModel logInRequest = new SerializeHelper().DeserializeObject<LoginModel>(Request.Body);
            bool isLoggingIn = logInRequest.IsLoggingIn;
            string direction = isLoggingIn ? "in" : "out";

            $"Got '{ logInRequest.Account.Name }' log { direction } request".WriteToConsole();

            string userAddress = GetUserAddress(Request);

            if (string.IsNullOrEmpty(userAddress))
            {
                $"Client { logInRequest.Account.Name } didnt attach his address".WriteToConsole();
                throw new InvalidOperationException("Client address must be set in headers");
            }

            ComplexAccountState accountState = LogInOrOut(logInRequest, userAddress);
            
            $"'{ logInRequest.Account.Name } [{ userAddress }]' log { direction } result: { accountState.StateModel.OnlineStatus }".WriteToConsole();

            var jsonResponse = new JsonResponse<AccountStateModel>(accountState.StateModel, new DefaultJsonSerializer());
            SetAuthHeaders(jsonResponse.Headers, accountState.ClientAuth);

            $"Logged '{ logInRequest.Account.Name }' { direction }".WriteToConsole(isLoggingIn ? ConsoleColor.Green : ConsoleColor.Magenta);

            return jsonResponse;
        }

        private string GetUserAddress(Request httpRequest) 
        {
            IEnumerable<string> headerValue = httpRequest.Headers[FreengyHeaders.ClientAddressHeaderName];

            return headerValue.First();
        }

        private void SetAuthHeaders(IDictionary<string, string> headers, SessionAuth auth) 
        {
            headers.Add(FreengyHeaders.ClientAddressHeaderName, auth.ClientToken);
            headers.Add(FreengyHeaders.ServerSessionTokenHeaderName, auth.ServerToken);
        }

        private ComplexAccountState LogInOrOut(LoginModel logInRequest, string userAddress)
        {
            bool isLoggingIn = logInRequest.IsLoggingIn;
            var stateService = AccountStateService.Instance;
            var targetStatus = isLoggingIn ? AccountOnlineStatus.Online : AccountOnlineStatus.Offline;

            ComplexAccountState complexAccountState;
            if (isLoggingIn)
            {
                complexAccountState = stateService.LogIn(logInRequest.Account.Name, userAddress);
            }
            else
            {
                var stateModel = stateService.LogOut(logInRequest.Account.Name);
                complexAccountState = new ComplexAccountState(stateModel);
            }

            if (complexAccountState.StateModel.OnlineStatus == targetStatus)
            {
                InformFriendsAboutLogin(complexAccountState);
            }

            return complexAccountState;
        }

        private void InformFriendsAboutLogin(ComplexAccountState complexState) 
        {
            var userId = complexState.StateModel.Account.Id;
            var friendships = FriendshipService.Instance.FindUserFriendships(userId);

            foreach (FriendshipModel friendship in friendships)
            {
                Guid friendId = friendship.AcceptorAccountId == userId ? friendship.ParentId : friendship.AcceptorAccountId;
                ComplexAccountState friendAccountState = AccountStateService.Instance.GetStatusOf(friendId);
                if(friendAccountState == null) throw new InvalidOperationException($"Got null friend state");

                if (friendAccountState.StateModel.OnlineStatus == AccountOnlineStatus.Online)
                {
                    using (IHttpActor actor = new HttpActor())
                    {
                        string targetFriendAddress = $"{ friendAccountState.StateModel.Address }{ Subroutes.NotifyClient.NotifyFriendState }";
                        actor.SetRequestAddress(targetFriendAddress);
                        actor.AddHeader(FreengyHeaders.ServerSessionTokenHeaderName, friendAccountState.ClientAuth.ServerToken);

                        var result = actor.PostAsync<AccountStateModel, AccountStateModel>(complexState.StateModel).Result;
                    }
                }
            }
        }
    }
}