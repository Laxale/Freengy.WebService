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
using Freengy.WebService.Services;

using Nancy;

using Newtonsoft.Json;


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
            Console.WriteLine($"Created { nameof(LogInModule) }");

            Post[Subroutes.Login] = OnLoginRequest;
        }


        private dynamic OnLoginRequest(dynamic arg) 
        {
            var stateService = AccountStateService.Instance;
            LoginModel logInRequest = new SerializeHelper().DeserializeObject<LoginModel>(Request.Body);

            string direction = logInRequest.IsLoggingIn ? "in" : "out";

            Console.WriteLine($"Got '{ logInRequest.Account.Name }' log { direction } request");

            string userAddress = GetUserAddress(Request);

            KeyValuePair<AccountStateModel, SessionAuth> resultPair = LogInOrOut(logInRequest, userAddress);
            
            Console.WriteLine($"'{ logInRequest.Account.Name }' log { direction } result: { resultPair.Key.OnlineStatus }");

            var jsonResponse = new JsonResponse<AccountStateModel>(resultPair.Key, new DefaultJsonSerializer());
            SetAuthHeaders(jsonResponse.Headers, resultPair.Value);

            Console.WriteLine($"Logged '{ logInRequest.Account.Name }' { direction }");

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

        private KeyValuePair<AccountStateModel, SessionAuth> LogInOrOut(LoginModel logInRequest, string userAddress)
        {
            bool isLoggingIn = logInRequest.IsLoggingIn;
            var stateService = AccountStateService.Instance;
            var targetStatus = isLoggingIn ? AccountOnlineStatus.Online : AccountOnlineStatus.Offline;

            KeyValuePair<AccountStateModel, SessionAuth> loginPair;
            if (isLoggingIn)
            {
                loginPair = stateService.LogIn(logInRequest.Account.Name, userAddress);
            }
            else
            {
                var auth = new SessionAuth();
                var stateModel = stateService.LogOut(logInRequest.Account.Name);
                loginPair = new KeyValuePair<AccountStateModel, SessionAuth>(stateModel, auth);
            }

            if (loginPair.Key.OnlineStatus == targetStatus)
            {
                InformFriendsAboutLogin(loginPair, isLoggingIn);
            }

            return loginPair;
        }

        private void InformFriendsAboutLogin(KeyValuePair<AccountStateModel, SessionAuth> friendIdLoginPair, bool isLoggedIn) 
        {
            var friendships = FriendshipService.Instance.FindByInvitor(friendIdLoginPair.Key.Account.Id);

            foreach (FriendshipModel friendship in friendships)
            {
                var valuePair = AccountStateService.Instance.GetStatusOf(friendship.AcceptorAccountId);
                if(valuePair == null) throw new InvalidOperationException($"Got null friend state");

                KeyValuePair<AccountStateModel, SessionAuth> friendStatePair = valuePair.Value;
                if (friendStatePair.Key.OnlineStatus == AccountOnlineStatus.Online)
                {
                    using (IHttpActor actor = new HttpActor())
                    {
                        string targetFriendAddress = $"{friendStatePair.Key.Address}/{Subroutes.NotifyClient.NotifyFriendState}/{valuePair.Value.Value.ClientToken}";
                        actor.SetRequestAddress(targetFriendAddress);
                        actor.AddHeader(FreengyHeaders.ServerSessionTokenHeaderName, friendIdLoginPair.Value.ServerToken);
                        actor.PostAsync<AccountStateModel, AccountStateModel>(friendStatePair.Key);
                    }
                }
            }
        }
    }
}