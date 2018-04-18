// Created by Laxale 18.04.2018
//
//

using System;

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
            var 
            Console.WriteLine("Got searching user request");

            throw new NotImplementedException();
        }
    }
}
