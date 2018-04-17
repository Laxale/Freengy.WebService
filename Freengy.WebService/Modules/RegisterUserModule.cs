// Created by Laxale 17.04.2018
//
//

using System;
using Freengy.WebService.Services;
using Nancy;
using Nancy.Security;
using NLog;

namespace Freengy.WebService.Modules
{
    public class RegisterUserModule : NancyModule 
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        public RegisterUserModule() 
        {
            //this.RequiresHttps();

            Console.WriteLine($"Created { nameof(RegisterUserModule) }");

            Post[Subroutes.Register] = OnRegisterRequest;
        }


        private dynamic OnRegisterRequest(dynamic argument) 
        {
            //logger.Info();
            Console.WriteLine("Incame register request:");

            var service = UserAccountService.Instance;

            return "Go register yourself";
        }
    }
}