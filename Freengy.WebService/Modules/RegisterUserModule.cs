// Created by Laxale 17.04.2018
//
//

using System;
using System.IO;

using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Models;
using Freengy.WebService.Services;

using NLog;

using Nancy;
using Nancy.Security;

using Newtonsoft.Json;


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

            RegistrationRequestModel registrationRequest = new SerializeHelper().DeserializeObject<RegistrationRequestModel>(Request.Body);
            
            var service = RegistrationService.Instance;

            RegistrationStatus registrationStatus = service.RegisterAccount(registrationRequest.UserName, out UserAccount registeredAcc);

            registrationRequest.Status = registrationStatus;
            registrationRequest.CreatedAccount = registeredAcc;

            Console.WriteLine($"Registered account '{ registrationRequest.UserName }' with result { registrationStatus }");
            string jsonResponce = JsonConvert.SerializeObject(registrationRequest, Formatting.Indented);

            return jsonResponce;
        }
    }
}