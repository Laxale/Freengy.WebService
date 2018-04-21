// Created by Laxale 19.04.2018
//
//

using Freengy.Common.Models;
using Freengy.WebService.Models;


namespace Freengy.WebService.Extensions 
{
    internal static class FriendRequestExtensions 
    {
        public static FriendRequest ToSimple(this ComplexFriendRequest complexRequest) 
        {
            var complex = new FriendRequest
            {
                CreationDate = complexRequest.CreationDate,
                Id = complexRequest.Id,
                RequesterAccount = complexRequest.NavigationParent,
                TargetAccount = complexRequest.TargetAccount,
                RequestState = complexRequest.RequestState
            };

            return complex;
        }
    }
}