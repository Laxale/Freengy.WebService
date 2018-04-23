// Created by Laxale 18.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;

using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Models;
using Freengy.WebService.Models;
using Freengy.WebService.Services;

using Nancy;


namespace Freengy.WebService.Modules 
{
    /// <summary>
    /// Searching module (whatever - users, session, etc).
    /// </summary>
    public class SearchModule : NancyModule 
    {
        public SearchModule() 
        {
            Console.WriteLine($"Created { nameof(SearchModule) }");

            Get[Subroutes.Search.Root] = OnRootSearchRequest;

            Post[Subroutes.Search.SearchUsers] = OnSearchUsersRequest;
        }


        private dynamic OnRootSearchRequest(dynamic arg) 
        {
            Console.WriteLine("Got root searching request");

            return $"Use {Subroutes.Search.SearchUsers} to search for users";
        }

        private dynamic OnSearchUsersRequest(dynamic arg) 
        {
            Console.WriteLine("Got searching users request");

            var helper = new SerializeHelper();
            var searchRequest = helper.DeserializeObject<SearchRequest>(Request.Body);

            if (searchRequest.SenderId == Guid.Empty)
            {
                throw new InvalidOperationException("Empty sender Id");
            }

            if (searchRequest.Entity == SearchEntity.Users)
            {
                return ProcessSearchUsers(searchRequest, helper);
            }

            if (searchRequest.Entity == SearchEntity.Friends)
            {
                return ProcessSearchFriends(searchRequest, helper);
            }

            // only user search for the moment
            throw new NotImplementedException();
        }

        private dynamic ProcessSearchUsers(SearchRequest searchRequest, SerializeHelper helper) 
        {
            IEnumerable<ComplexUserAccount> users =
                RegistrationService
                    .Instance
                    .FindByNameFilter(searchRequest.SearchFilter)
                    //.Where(acc => acc.UniqueId != searchRequest.SenderId);
                    .Where(acc => acc.Id != searchRequest.SenderId);

            var responce = helper.Serialize(users);

            return responce;
        }

        private dynamic ProcessSearchFriends(SearchRequest searchRequest, SerializeHelper helper) 
        {
            ComplexUserAccount senderAcc = RegistrationService.Instance.FindById(searchRequest.SenderId);

            if (senderAcc == null)
            {
                return $"Account id {searchRequest.SenderId} is not registered";
            }

            var stateService = AccountStateService.Instance;

            AccountStateModel AccountStateSelector(FriendshipModel friendship)
            {
                ComplexUserAccount account = 
                    friendship.AcceptorAccountId == searchRequest.SenderId ? 
                        friendship.NavigationParent : 
                        friendship.AcceptorAccount;

                AccountStateModel accountState = stateService.GetStatusOf(account.Id);

                return accountState;
            }

            var friends = senderAcc.Friendships.Select(AccountStateSelector);

            var serialized = helper.Serialize(friends);

            return serialized;
        }
    }
}