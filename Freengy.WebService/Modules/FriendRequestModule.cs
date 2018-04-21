// Created by Laxale 19.04.2018
//
//

using System;

using Freengy.Common.Enums;
using Freengy.Common.Models;
using Freengy.Common.Helpers;
using Freengy.WebService.Models;
using Freengy.WebService.Services;

using Nancy;


namespace Freengy.WebService.Modules 
{
    using System.Collections.Generic;
    using System.Linq;

    using Freengy.WebService.Exceptions;
    using Freengy.WebService.Extensions;


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
            Post[Subroutes.Search.SearchFriendRequests] = OnSearchFriendRequest;
        }


        private dynamic OnSearchFriendRequest(dynamic arg) 
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
    }
}