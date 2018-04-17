// Created by Laxale 17.04.2018
//
//

using System;

using Freengy.WebService.Enums;
using Freengy.WebService.Helpers;
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
            Console.WriteLine("Got log in request");

            var accountService = AccountStateService.Instance;

            LogInRequest logInRequest = new SerializeHelper().DeserializeObject<LogInRequest>(Request.Body);

            AccountOnlineStatus result = accountService.LogIn(logInRequest.UserName);

            logInRequest.LogInStatus = result;

            string responce = JsonConvert.SerializeObject(logInRequest, Formatting.Indented);

            return responce;
        }
    }
}