// Created by Laxale 10.05.2018
//
//

using System;

using Freengy.Common.Extensions;
using Freengy.Common.Helpers;
using Freengy.Common.Models;
using Freengy.WebService.Exceptions;
using Freengy.WebService.Models;
using Freengy.WebService.Services;

using Nancy;


namespace Freengy.WebService.Modules 
{
    /// <summary>
    /// Module for handling Edit requests.
    /// </summary>
    public class EditModule : NancyModule 
    {
        private Guid requesterId;


        public EditModule() 
        {
            //Before.AddItemToStartOfPipeline(ValidateRequest);

            Post[Subroutes.Edit.EditAccount] = OnEditAccount;
        }


        private dynamic OnEditAccount(dynamic arg) 
        {
            var editRequest = new SerializeHelper().DeserializeObject<EditAccountModel>(Request.Body);

            Guid senderId = Request.Headers.GetClientId();
            var stateService = AccountStateService.Instance;

            SessionAuth clientAuth = Request.Headers.GetSessionAuth();
            if (!stateService.IsAuthorized(senderId, clientAuth.ClientToken))
            {
                return HttpStatusCode.Forbidden;
            }

            ComplexAccountState editedAccount = stateService.GetStatusOf(senderId);
            //editedAccount.StateModel.
            RegistrationService.Instance.UpdateAccountProps(senderId, editRequest);

            return HttpStatusCode.Accepted;
        }
    }
}