// Created by Laxale 10.05.2018
//
//

using System;

using Freengy.Common.Constants;
using Freengy.Common.Extensions;
using Freengy.Common.Models;
using Freengy.WebService.Exceptions;
using Freengy.WebService.Models;
using Freengy.WebService.Services;

using Nancy;
using Nancy.Responses;


namespace Freengy.WebService.Modules 
{
    /// <summary>
    /// Module for handling personal client sync requests.
    /// </summary>
    public class SyncModule : NancyModule 
    {
        public SyncModule() 
        {
             Get[Subroutes.Sync.SyncAccount] = OnSyncAccountRequest;
        }


        private dynamic OnSyncAccountRequest(dynamic arg) 
        {
            var stateService = AccountStateService.Instance;

            Guid senderId = Request.Headers.GetClientId();
            SessionAuth clientAuth = Request.Headers.GetSessionAuth();

            if (!stateService.IsAuthorized(senderId, clientAuth.ClientToken))
            {
                throw new ClientNotAuthorizedException(senderId);
            }

            ComplexAccountState currentState = AccountStateService.Instance.GetStatusOf(senderId);

            var responce = new JsonResponse<AccountStateModel>(currentState.StateModel, new DefaultJsonSerializer());
            responce.Headers.Add(FreengyHeaders.Server.ServerSessionTokenHeaderName, currentState.ClientAuth.ServerToken);

            return responce;
        }
    }
}