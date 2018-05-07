// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;

using Freengy.Common.Constants;
using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Interfaces;
using Freengy.Common.Models;
using Freengy.Common.Models.Readonly;
using Freengy.WebService.Extensions;
using Freengy.WebService.Helpers;
using Freengy.WebService.Services;
using Freengy.WebService.Models;

using Nancy;
using Nancy.Responses;


namespace Freengy.WebService.Modules 
{
    /// <summary>
    /// Module for user log in action.
    /// </summary>
    public class LogInModule : NancyModule 
    {
        private LoginModel logInRequest;

        
        public LogInModule() 
        {
            $"Created { nameof(LogInModule) }".WriteToConsole();

            Before.AddItemToStartOfPipeline(ValidateUserPassword);

            Post[Subroutes.Login] = OnLoginRequest;
        }


        private Response ValidateUserPassword(NancyContext nancyContext) 
        {
            logInRequest = new SerializeHelper().DeserializeObject<LoginModel>(Request.Body);
            var realAccount = RegistrationService.Instance.FindByName(logInRequest.Account.Name);

            bool isPasswordValid = PasswordService.Instance.ValidatePassword(realAccount.Id, logInRequest.PasswordHash);

            if (!isPasswordValid)
            {
                logInRequest.LogInStatus = AccountOnlineStatus.InvalidPassword;
                $"{ logInRequest.Account.Name } provided invalid password".WriteToConsole(ConsoleColor.Red);
                return HttpStatusCode.Forbidden;
            }

            return null;
        }
        
        private dynamic OnLoginRequest(dynamic arg) 
        {
            //logInRequest = new SerializeHelper().DeserializeObject<LoginModel>(Request.Body);
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

            // сложный аккаунт нужно упростить, иначе при дефолтной сериализации получается бесконечная рекурсия ссылающихся друг на друга свойств
            if (accountState.StateModel.Account is ComplexUserAccount complexAcc)
            {
                accountState.StateModel.Account = complexAcc.ToSimpleModel();
            }
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
            var informer = UserInformerService.Instance;
            Guid userId = complexState.StateModel.Account.Id;
            IEnumerable<FriendshipModel> friendships = FriendshipService.Instance.FindAllFriendships(userId);

            foreach (FriendshipModel friendship in friendships)
            {
                Guid friendId = friendship.AcceptorAccountId == userId ? friendship.ParentId : friendship.AcceptorAccountId;
                informer.NotifyFriendAboutLogin(friendId, complexState);
            }
        }
    }
}