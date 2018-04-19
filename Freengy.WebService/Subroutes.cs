// Created by Laxale 17.04.2018
//
//


namespace Freengy.WebService 
{
    /// <summary>
    /// Contains constant service url subroutes.
    /// </summary>
    internal class Subroutes 
    {
        /// <summary>
        /// Hello module subroute.
        /// </summary>
        public static readonly string Hello = "/hello";

        /// <summary>
        /// Registration module subroute.
        /// </summary>
        public static readonly string Register = "/register";

        /// <summary>
        /// Log-in module subroute.
        /// </summary>
        public static readonly string Login = "/login";


        /// <summary>
        /// Contains search module subroutes.
        /// </summary>
        public static class Search 
        {
            /// <summary>
            /// Root Search module subroute.
            /// </summary>
            public static readonly string Root = "/search";

            /// <summary>
            /// Search users subroute.
            /// </summary>
            public static readonly string SearchUsers = $"{ Root }/users";
        }


        /// <summary>
        /// Contains request subroutes.
        /// </summary>
        public static class Request 
        {
            /// <summary>
            /// Root Search module subroute.
            /// </summary>
            public static readonly string Root = "/request";

            public static readonly string AddFriend = $"{ Root }/friend";
        }
    }
}