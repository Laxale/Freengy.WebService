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

            AccountStateModel stateModel = 
                logInRequest.IsLoggingIn ? 
                    stateService.LogIn(logInRequest.Account.Name, userAddress) : 
                    stateService.LogOut(logInRequest.Account.Name);

            Console.WriteLine($"'{ logInRequest.Account.Name }' log in result: { stateModel.OnlineStatus }");

            string responce = JsonConvert.SerializeObject(stateModel, Formatting.Indented);

            Console.WriteLine($"Logged '{ logInRequest.Account.Name }' { direction }");

            return responce;
        }

        private string GetUserAddress(Request httpRequest) 
        {
            IEnumerable<string> headerValue = httpRequest.Headers[FreengyHeaders.ClientAddressHeaderName];

            return headerValue.First();
        }
    }
}