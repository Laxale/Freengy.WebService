// Created by Laxale 17.04.2018
//
//

using System;

using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Models;
using Freengy.WebService.Models;
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
            var accountService = AccountStateService.Instance;

            LoginModel logInRequest = new SerializeHelper().DeserializeObject<LoginModel>(Request.Body);

            Console.WriteLine($"Got '{ logInRequest.UserName }' log in request");

            AccountOnlineStatus result = accountService.LogIn(logInRequest.UserName);

            logInRequest.LogInStatus = result;

            Console.WriteLine($"'{ logInRequest.UserName }' log in result: { result }");

            string responce = JsonConvert.SerializeObject(logInRequest, Formatting.Indented);

            return responce;
        }
    }
}