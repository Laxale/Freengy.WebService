// Created by Laxale 17.04.2018
//
//

using System;
using System.IO;

using Freengy.Common.Constants;
using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Models;
using Freengy.WebService.Extensions;
using Freengy.WebService.Helpers;
using Freengy.WebService.Models;
using Freengy.WebService.Services;

using NLog;

using Nancy;
using Nancy.Responses;
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
            "Incame register request:".WriteToConsole();

            var helper = new SerializeHelper();
            var registrationRequest = helper.DeserializeObject<RegistrationRequest>(Request.Body);
            
            var service = RegistrationService.Instance;

            RegistrationStatus registrationStatus = service.RegisterAccount(registrationRequest, out ComplexUserAccount registeredAcc);

            registrationRequest.Status = registrationStatus;
            
            $"Registered account '{ registrationRequest.UserName }' with result { registrationStatus }".WriteToConsole();

            var jsonResponce = new JsonResponse<RegistrationRequest>(registrationRequest, new DefaultJsonSerializer());
            if (registeredAcc != null)
            {
                registrationRequest.CreatedAccount = registeredAcc.ToSimpleModel();
                jsonResponce.Headers.Add(FreengyHeaders.Server.NextPasswordSaltHeaderName, registeredAcc.PasswordData.NextLoginSalt);
            }

            return jsonResponce;
        }
    }
}