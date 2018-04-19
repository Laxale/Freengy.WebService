// Created by Laxale 18.04.2018
//
//

using System;
using System.Collections.Generic;
using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Models;
using Freengy.WebService.Services;
using Nancy;
using Newtonsoft.Json;


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

            if (searchRequest.Entity == SearchEntity.Users)
            {
                IEnumerable<UserAccount> users = RegistrationService.Instance.FindByNameFilter(searchRequest.SearchFilter);

                var responce = helper.Serialize(users);

                return responce;
            }

            // only user search for the moment
            throw new NotImplementedException();
        }
    }
}
