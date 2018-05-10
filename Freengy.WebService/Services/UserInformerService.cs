// Created by Laxale 07.05.2018
//
//

using System;
using Freengy.Common.Constants;
using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Interfaces;
using Freengy.Common.Models;
using Freengy.WebService.Helpers;
using Freengy.WebService.Interfaces;
using Freengy.WebService.Models;


namespace Freengy.WebService.Services 
{
    internal class UserInformerService : IService 
    {
        private static readonly object Locker = new object();

        private static UserInformerService instance;


        private UserInformerService() { }


        /// <summary>
        /// Единственный инстанс <see cref="UserInformerService"/>.
        /// </summary>
        public static UserInformerService Instance 
        {
            get
            {
                lock (Locker)
                {
                    return instance ?? (instance = new UserInformerService());
                }
            }
        }


        /// <summary>
        /// Initialize the service.
        /// </summary>
        public void Initialize() { }

        /// <summary>
        /// Send informing message to a friend about user login.
        /// </summary>
        /// <param name="userState"></param>
        /// <param name="friendId"></param>
        public void NotifyFriendAboutLogin(Guid friendId, ComplexAccountState userState) 
        {
            ComplexAccountState friendAccountState = AccountStateService.Instance.GetStatusOf(friendId);
            if (friendAccountState == null) throw new InvalidOperationException($"Got null friend state");

            if (friendAccountState.StateModel.OnlineStatus == AccountOnlineStatus.Online)
            {
                using (IHttpActor actor = new HttpActor())
                {
                    string targetFriendAddress = $"{friendAccountState.StateModel.Address}{Subroutes.NotifyClient.NotifyFriendState}";
                    actor.SetRequestAddress(targetFriendAddress);
                    actor.AddHeader(FreengyHeaders.Server.ServerSessionTokenHeaderName, friendAccountState.ClientAuth.ServerToken);

                    var result = actor.PostAsync<AccountStateModel, AccountStateModel>(userState.StateModel).Result;
                }
            }
        }

        /// <summary>
        /// Notify user about appeared reaction for his friendrequest.
        /// </summary>
        /// <param name="requesterId"></param>
        /// <param name="requestReply"></param>
        public void NotifyUserAboutFriendReply(Guid requesterId, FriendRequestReply requestReply) 
        {
            ComplexAccountState requesterState = AccountStateService.Instance.GetStatusOf(requesterId);
            if (requesterState == null) throw new InvalidOperationException($"Got null requester state");

            if (requesterState.StateModel.OnlineStatus == AccountOnlineStatus.Online)
            {
                using (IHttpActor actor = new HttpActor())
                {
                    string requesterAddress = $"{requesterState.StateModel.Address}{Subroutes.NotifyClient.NotifyFriendRequestState}";
                    actor.SetRequestAddress(requesterAddress);
                    actor.AddHeader(FreengyHeaders.Server.ServerSessionTokenHeaderName, requesterState.ClientAuth.ServerToken);

                    var result = actor.PostAsync<FriendRequestReply, FriendRequestReply>(requestReply).Result;

                    string resultMessage = result.Success ? "Success" : result.Error.Message;

                    $"Sent friendrequest reply '{ requestReply.Reaction }' to { requesterState.StateModel.Account.Name }:{ Environment.NewLine }    { resultMessage }"
                        .WriteToConsole(ConsoleColor.Blue);
                }
            }
        }

        public void NotifyUserAboutFriendRequest(Guid userId, FriendRequest request) 
        {
            ComplexAccountState targetUserState = AccountStateService.Instance.GetStatusOf(userId);
            if (targetUserState == null) throw new InvalidOperationException($"Got null target user state");

            if (targetUserState.StateModel.OnlineStatus == AccountOnlineStatus.Online)
            {
                using (IHttpActor actor = new HttpActor())
                {
                    string requesterAddress = $"{ targetUserState.StateModel.Address }{ Subroutes.NotifyClient.NotifyFriendRequest }";
                    actor.SetRequestAddress(requesterAddress);
                    actor.AddHeader(FreengyHeaders.Server.ServerSessionTokenHeaderName, targetUserState.ClientAuth.ServerToken);

                    var result = actor.PostAsync<FriendRequest, FriendRequest>(request).Result;

                    string resultMessage = result.Success ? "Success" : result.Error.Message;

                    (
                        $"Sent friendrequest notification from '{ request.RequesterAccount.Name }' to " +
                        $"{ request.TargetAccount.Name }:{ Environment.NewLine }    { resultMessage }"
                    )
                    .WriteToConsole(ConsoleColor.DarkBlue);
                }
            }
        }
    }
}