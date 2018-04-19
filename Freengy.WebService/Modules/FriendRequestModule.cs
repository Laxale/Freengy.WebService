// Created by Laxale 19.04.2018
//
//

using System;

using Nancy;


namespace Freengy.WebService.Modules 
{
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


            return "fuk";
        }
    }
}