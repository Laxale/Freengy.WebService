// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Freengy.Common.Constants;
using Freengy.Common.Enums;
using Freengy.Common.Helpers;
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
            var serializer = new SerializeHelper();
            LoginModel logInRequest = serializer.DeserializeObject<LoginModel>(Request.Body);

            string direction = logInRequest.IsLoggingIn ? "in" : "out";

            Console.WriteLine($"Got '{ logInRequest.Account.Name }' log { direction } request");

            string userAddress = GetUserAddress(Request);

            SessionAuth auth;
            AccountStateModel stateModel;
            if (logInRequest.IsLoggingIn)
            {
                KeyValuePair<AccountStateModel, SessionAuth> loginPair = stateService.LogIn(logInRequest.Account.Name, userAddress);
                auth = loginPair.Value;
                stateModel = loginPair.Key;
            }
            else
            {
                auth = new SessionAuth();
                stateModel = stateService.LogOut(logInRequest.Account.Name);
            }
            
            Console.WriteLine($"'{ logInRequest.Account.Name }' log { direction } result: { stateModel.OnlineStatus }");

            //string responce = serializer.Serialize(stateModel);
            var jsonResponse = new JsonResponse<AccountStateModel>(stateModel, new DefaultJsonSerializer());
            SetAuthHeaders(jsonResponse.Headers, auth);

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
    }
}