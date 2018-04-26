// Created by Laxale 17.04.2018
//
//


using System.Runtime.Remoting.Metadata.W3cXsd2001;

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

            /// <summary>
            /// Search friend requests subroute.
            /// </summary>
            public static readonly string SearchFriendRequests = $"{ Root }/friendrequest";
        }


        /// <summary>
        /// Contains request subroutes.
        /// </summary>
        public static class Request 
        {
            /// <summary>
            /// Root Request module subroute.
            /// </summary>
            public static readonly string Root = "/request";

            public static readonly string AddFriend = $"{ Root }/friend";
        }

        /// <summary>
        /// Contains reply subroutes.
        /// </summary>
        public static class Reply 
        {
            /// <summary>
            /// Root Reply module subroute.
            /// </summary>
            public static readonly string Root = "/reply";

            public static readonly string FriendRequest = $"{ Root }/friendrequest";
        }

        public static class NotifyClient
        {
            private static readonly string inform = "inform";

            public static readonly string Root = "/fromserver";

            public static readonly string NotifyFriendState = $"{Root}/{inform}/friend/state";
        }
    }
}