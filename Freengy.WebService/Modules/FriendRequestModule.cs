// Created by Laxale 19.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;

using Freengy.Common.Enums;
using Freengy.Common.Models;
using Freengy.Common.Helpers;
using Freengy.WebService.Models;
using Freengy.WebService.Services;
using Freengy.WebService.Extensions;
using Freengy.WebService.Exceptions;

using Nancy;


namespace Freengy.WebService.Modules 
{
    /// <summary>
    /// Friend request handling module.
    /// </summary>
    public class FriendRequestModule : NancyModule 
    {
        public FriendRequestModule() 
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Created { nameof(FriendRequestModule) }");
            Console.ForegroundColor = ConsoleColor.White;

            Post[Subroutes.Request.AddFriend] = OnAddFriendRequest;
            Post[Subroutes.Search.SearchFriendRequests] = OnSearchFriendRequestRequest;
            Post[Subroutes.Reply.FriendRequest] = OnFriendRequestReply;
        }


        private dynamic OnAddFriendRequest(dynamic arg) 
        {
            var friendRequest = new SerializeHelper().DeserializeObject<FriendRequest>(Request.Body);

            Console.WriteLine($"Got a friend request from { friendRequest.RequesterAccount.Name } to { friendRequest.TargetAccount.Name }");

            RegistrationService registrationService = RegistrationService.Instance;
            FriendRequestService friendRequestService = FriendRequestService.Instance;
            
            if(registrationService.FindByName(friendRequest.RequesterAccount.Name) != null)
            {
                ComplexFriendRequest result = friendRequestService.Save(friendRequest);

                friendRequest.RequestState = result.RequestState;

                Console.WriteLine($"Saved request with result '{ friendRequest.RequestState }'");

                string serialized = new SerializeHelper().Serialize(friendRequest);

                return serialized;
            }

            Console.WriteLine($"Target acount { friendRequest.TargetAccount.Name } doesnt exist");

            friendRequest.RequestState = FriendRequestState.DoesntExist;

            return friendRequest;
        }

        private dynamic OnFriendRequestReply(dynamic arg) 
        {
            var helper = new SerializeHelper();
            var requestReply = helper.DeserializeObject<FriendRequestReply>(Request.Body);

            Console.WriteLine($"Got a friend request reply from { requestReply.Request.TargetAccount.Name } to { requestReply.Request.RequesterAccount.Name }");

            //var senderId = Guid.Parse(requestReply.Id);
            var senderId = requestReply.Id;
            if (!AccountStateService.Instance.IsAuthorized(senderId, requestReply.UserToken))
            {
                throw new NotAuthorizedException(senderId);
            }

            FriendRequestService friendRequestService = FriendRequestService.Instance;

            ComplexFriendRequest processedRequest = friendRequestService.ReplyToRequest(requestReply);
            requestReply.EstablishedDate = processedRequest.DecisionDate;
            var serialized = helper.Serialize(requestReply);

            return serialized;
        }

        private dynamic OnSearchFriendRequestRequest(dynamic arg) 
        {
            var searchRequest = new SerializeHelper().DeserializeObject<SearchRequest>(Request.Body);
            SearchEntity searchEntity = searchRequest.Entity;

            if (searchEntity != SearchEntity.IncomingFriendRequests &&
                searchEntity != SearchEntity.OutgoingFriendRequests)
            {
                throw new InvalidOperationException($"Search entity '{ searchEntity }' must not be sent to this controller");
            }

            AccountStateService stateService = AccountStateService.Instance;
            FriendRequestService requestService = FriendRequestService.Instance;

            if (!stateService.IsAuthorized(searchRequest.SenderId, searchRequest.UserToken))
            {
                throw new NotAuthorizedException(searchRequest.SenderId);
            }

            IEnumerable<ComplexFriendRequest> requests = 
                searchEntity == SearchEntity.IncomingFriendRequests ? 
                    requestService.GetIncomingRequests(searchRequest.SenderId) : 
                    requestService.GetOutgoingRequests(searchRequest.SenderId);

            IEnumerable<FriendRequest> clientModels = requests.Select(complexModel => complexModel.ToSimple());
            string serialized = new SerializeHelper().Serialize(clientModels);

            return serialized;
        }
    }
}