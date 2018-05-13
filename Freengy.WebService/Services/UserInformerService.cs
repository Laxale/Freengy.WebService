// Created by Laxale 07.05.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Freengy.Common.Constants;
using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Interfaces;
using Freengy.Common.Models;
using Freengy.WebService.Extensions;
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

        public void NotifyUserOfExpAddition(ComplexAccountState userState, uint expAmount) 
        {
            using (IHttpActor actor = new HttpActor())
            {
                //TODO добавить начисление экспы
                string address = $"{userState.Address}{Subroutes.NotifyClient.}";
                actor.SetRequestAddress(address);
                actor.AddHeader(FreengyHeaders.Server.ServerSessionTokenHeaderName, userState.ClientAuth.ServerToken);

                var result = actor.PostAsync<, >(userState.ToSimple()).Result;
            }
        }

        /// <summary>
        /// Send informing message to a friend about user account state change.
        /// </summary>
        /// <param name="userState"></param>
        /// <param name="friendId"></param>
        public void NotifyFriendAboutUserChange(Guid friendId, ComplexAccountState userState) 
        {
            ComplexAccountState friendAccountState = AccountStateService.Instance.GetStatusOf(friendId);
            if (friendAccountState == null) throw new InvalidOperationException($"Got null friend state");

            if (friendAccountState.OnlineStatus == AccountOnlineStatus.Online)
            {
                NotifyFriendAboutUser(userState, friendAccountState);
            }
        }

        /// <summary>
        /// Отправить уведомления всем друзьям юзера о любом изменении состояния его аккаунта.
        /// </summary>
        /// <param name="userState">Модель состояния аккаунта юзера.</param>
        public void NotifyAllFriendsAboutUser(ComplexAccountState userState) 
        {
            var outFriendIds = 
                FriendshipService.Instance.FindByInvoker(userState.ComplexAccount.Id)
                .Select(friendship => friendship.AcceptorAccountId);

            var inFriendIds =
                FriendshipService.Instance.FindByAcceptor(userState.ComplexAccount.Id)
                    .Select(friendship => friendship.ParentId);

            var allFriendIds = inFriendIds.Union(outFriendIds);
            IEnumerable<ComplexAccountState> allFriendStates = allFriendIds.Select(friendId => AccountStateService.Instance.GetStatusOf(friendId));

            Parallel.ForEach(allFriendStates, friendState =>
            {
                if (friendState.OnlineStatus == AccountOnlineStatus.Online)
                {
                    NotifyFriendAboutUser(userState, friendState);
                }
            });
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

            if (requesterState.OnlineStatus == AccountOnlineStatus.Online)
            {
                using (IHttpActor actor = new HttpActor())
                {
                    string requesterAddress = $"{requesterState.Address}{Subroutes.NotifyClient.NotifyFriendRequestState}";
                    actor.SetRequestAddress(requesterAddress);
                    actor.AddHeader(FreengyHeaders.Server.ServerSessionTokenHeaderName, requesterState.ClientAuth.ServerToken);

                    var result = actor.PostAsync<FriendRequestReply, FriendRequestReply>(requestReply).Result;

                    string resultMessage = result.Success ? "Success" : result.Error.Message;

                    $"Sent friendrequest reply '{ requestReply.Reaction }' to { requesterState.ComplexAccount.Name }:{ Environment.NewLine }    { resultMessage }"
                        .WriteToConsole(ConsoleColor.Blue);
                }
            }
        }

        /// <summary>
        /// Сообщить пользователю о новом запросе в друзья.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя, которому поступил запрос в друзья.</param>
        /// <param name="request">Поступивший запрос в друзья.</param>
        public void NotifyUserAboutFriendRequest(Guid userId, FriendRequest request) 
        {
            ComplexAccountState targetUserState = AccountStateService.Instance.GetStatusOf(userId);
            if (targetUserState == null) throw new InvalidOperationException($"Got null target user state");

            if (targetUserState.OnlineStatus == AccountOnlineStatus.Online)
            {
                using (IHttpActor actor = new HttpActor())
                {
                    string requesterAddress = $"{ targetUserState.Address }{ Subroutes.NotifyClient.NotifyFriendRequest }";
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


        private static void NotifyFriendAboutUser(ComplexAccountState userState, ComplexAccountState friendAccountState) 
        {
            using (IHttpActor actor = new HttpActor())
            {
                string targetFriendAddress = $"{friendAccountState.Address}{Subroutes.NotifyClient.NotifyFriendState}";
                actor.SetRequestAddress(targetFriendAddress);
                actor.AddHeader(FreengyHeaders.Server.ServerSessionTokenHeaderName, friendAccountState.ClientAuth.ServerToken);

                var result = actor.PostAsync<AccountStateModel, AccountStateModel>(userState.ToSimple()).Result;
            }
        }
    }
}