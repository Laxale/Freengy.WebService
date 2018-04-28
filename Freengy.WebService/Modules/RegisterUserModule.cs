// Created by Laxale 17.04.2018
//
//

using System;
using System.IO;

using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Models;
using Freengy.WebService.Helpers;
using Freengy.WebService.Models;
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

            $"Created { nameof(RegisterUserModule) }".WriteToConsole();

            Post[Subroutes.Register] = OnRegisterRequest;
        }


        private dynamic OnRegisterRequest(dynamic argument) 
        {
            //logger.Info();
            "Incame register request:".WriteToConsole();

            var helper = new SerializeHelper();
            var registrationRequest = helper.DeserializeObject<RegistrationRequest>(Request.Body);
            
            var service = RegistrationService.Instance;

            RegistrationStatus registrationStatus = service.RegisterAccount(registrationRequest.UserName, out ComplexUserAccount registeredAcc);

            registrationRequest.Status = registrationStatus;
            registrationRequest.CreatedAccount = registeredAcc;

            $"Registered account '{ registrationRequest.UserName }' with result { registrationStatus }".WriteToConsole();
            string jsonResponce = helper.Serialize(registrationRequest);

            return jsonResponce;
        }
    }
}