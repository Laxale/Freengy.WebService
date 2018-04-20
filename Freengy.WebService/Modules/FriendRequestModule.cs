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
            Console.WriteLine($"Created { nameof(FriendRequestModule) }");

            Post[Subroutes.Request.AddFriend] = OnAddFriendRequest;
        }


        private dynamic OnAddFriendRequest(dynamic arg) 
        {
            Console.WriteLine($"Got new friend request");
            var friendRequest = new SerializeHelper().DeserializeObject<FriendRequest>(Request.Body);

            RegistrationService registrationService = RegistrationService.Instance;
            FriendRequestService friendRequestService = FriendRequestService.Instance;
            
            if(registrationService.FindByName(friendRequest.RequesterAccount.Name) != null)
            {
                ComplexFriendRequest result = friendRequestService.Save(friendRequest);

                friendRequest.RequestState = result.RequestState;

                string serialized = new SerializeHelper().Serialize(result);

                return serialized;
            }

            friendRequest.RequestState = FriendRequestState.DoesntExist;

            return friendRequest;
        }
    }
}