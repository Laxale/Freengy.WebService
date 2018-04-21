// Created by Laxale 19.04.2018
//
//

using System;
using Freengy.Common.Enums;
using Freengy.Common.Models;
using Freengy.Common.Helpers;

using Nancy;


namespace Freengy.WebService.Modules 
{
    using Freengy.WebService.Models;
    using Freengy.WebService.Services;


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