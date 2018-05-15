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

            /// <summary>
            /// Search friend requests subroute.
            /// </summary>
            public static readonly string SearchFriendRequests = $"{ Root }/friendrequest";

            /// <summary>
            /// Search user avatars subroute.
            /// </summary>
            public static readonly string SearchUserAvatars = $"{ Root }/avatar";
        }

        /// <summary>
        /// Contains edit entities subroutes.
        /// </summary>
        public static class Edit 
        {
            /// <summary>
            /// Root Edit module subroute.
            /// </summary>
            public static readonly string Root = "/edit";

            public static readonly string EditAccount = $"{Root}/account";

            public static readonly string EditAccountName = $"{Root}/account/name";

            public static readonly string EditAccountImage = $"{Root}/account/image";
        }

        /// <summary>
        /// Contains synchronization requests subroutes.
        /// </summary>
        public static class Sync 
        {
            /// <summary>
            /// Root Sync module subroute.
            /// </summary>
            public static readonly string Root = "/sync";

            public static readonly string SyncAccount = $"{Root}/account";
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

        /// <summary>
        /// Contains subroutes to notify clients.
        /// </summary>
        public static class NotifyClient 
        {
            private static readonly string inform = "inform";

            public static readonly string Root = "fromserver";

            public static readonly string SyncExp = $"{ Root }/{ inform }/exp";

            public static readonly string NotifyFriendState = $"{Root}/{inform}/friend/state";

            public static readonly string RequestUserState = $"{Root}/replystate";

            public static readonly string NotifyFriendRequest = $"{Root}/{ inform }/friendrequest";

            public static readonly string NotifyFriendRequestState = $"{ NotifyFriendRequest }/state";
        }
    }
}