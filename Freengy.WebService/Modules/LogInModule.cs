// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.ObjectModel;
using Freengy.WebService.Helpers;
using Freengy.WebService.Models;
using Freengy.WebService.Modules;

using Nancy;


namespace Freengy.WebService.Services 
{
    /// <summary>
    /// Module for user log in action.
    /// </summary>
    internal class LogInModule : NancyModule 
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

            RegistrationRequest registrationRequest = new SerializeHelper().DeserializeObject<RegistrationRequest>(Request.Body);

            accountService.LogIn()

            return "yep!";
        }
    }
}