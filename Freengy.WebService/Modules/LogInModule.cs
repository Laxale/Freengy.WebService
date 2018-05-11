// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            if (realAccount == null)
            {
                return HttpStatusCode.Unauthorized;
            }

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
            
            $"'{ logInRequest.Account.Name } [{ userAddress }]' log { direction } result: { accountState.OnlineStatus }".WriteToConsole();

            // сложный аккаунт нужно упростить, иначе при дефолтной сериализации получается бесконечная рекурсия ссылающихся друг на друга свойств
            
            var jsonResponse = new JsonResponse<AccountStateModel>(accountState.ToSimple(), new DefaultJsonSerializer());
            SetAuthHeaders(jsonResponse.Headers, accountState.ClientAuth);

            $"Logged '{ logInRequest.Account.Name }' { direction }".WriteToConsole(isLoggingIn ? ConsoleColor.Green : ConsoleColor.Magenta);

            return jsonResponse;
        }

        private string GetUserAddress(Request httpRequest) 
        {
            IEnumerable<string> headerValue = httpRequest.Headers[FreengyHeaders.Client.ClientAddressHeaderName];

            return headerValue.First();
        }

        private void SetAuthHeaders(IDictionary<string, string> headers, SessionAuth auth) 
        {
            headers.Add(FreengyHeaders.Client.ClientAddressHeaderName, auth.ClientToken);
            headers.Add(FreengyHeaders.Server.ServerSessionTokenHeaderName, auth.ServerToken);
        }

        private ComplexAccountState LogInOrOut(LoginModel logInRequest, string userAddress)
        {
            bool isLoggingIn = logInRequest.IsLoggingIn;
            var stateService = AccountStateService.Instance;
            var targetStatus = isLoggingIn ? AccountOnlineStatus.Online : AccountOnlineStatus.Offline;

            ComplexAccountState complexAccountState = 
                isLoggingIn ? 
                    stateService.LogIn(logInRequest.Account.Name, userAddress) : 
                    stateService.LogOut(logInRequest.Account.Name);

            if (complexAccountState.OnlineStatus == targetStatus)
            {
                Task.Run(() => UserInformerService.Instance.NotifyAllFriendsAboutUser(complexAccountState));
            }

            return complexAccountState;
        }
    }
}