// Created by Laxale 19.04.2018
//
//

using System;
using System.Linq;
using System.Collections.Generic;

using Freengy.Common.Enums;
using Freengy.Database.Context;
using Freengy.WebService.Context;
using Freengy.WebService.Models;
using Freengy.Common.Models;
using Freengy.WebService.Extensions;
using Freengy.WebService.Interfaces;

using NLog;


namespace Freengy.WebService.Services 
{
    using System.Data.Entity;


    internal class FriendRequestService : IService 
    {
        private static readonly object Locker = new object();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static FriendRequestService instance;
        

        private FriendRequestService() 
        {
            
        }


        /// <summary>
        /// Единственный инстанс <see cref="FriendRequestService"/>.
        /// </summary>
        public static FriendRequestService Instance 
        {
            get
            {
                lock (Locker)
                {
                    return instance ?? (instance = new FriendRequestService());
                }
            }
        }


        /// <inheritdoc />
        /// <summary>
        /// Initialize the service.
        /// </summary>
        public void Initialize() 
        {
            try
            {
                using (var dbContext = new ComplexFriendRequestContext())
                {
                    var friendRequest = dbContext.Objects.FirstOrDefault();

                    var test = friendRequest?.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.Error(ex, $"Failed to initialize { nameof(FriendRequestService) } service");
            }
        }

        /// <summary>
        /// Save a friend request to database.
        /// </summary>
        /// <param name="request">Incoming friend request model.</param>
        /// <returns>Saved complex request model.</returns>
        public ComplexFriendRequest Save(FriendRequest request) 
        {
            RegistrationService service = RegistrationService.Instance;
            //ComplexUserAccount requester = service.FindById(request.RequesterAccount.UniqueId);
            ComplexUserAccount requester = service.FindById(request.RequesterAccount.Id);
            //ComplexUserAccount target = service.FindById(request.TargetAccount.UniqueId);
            ComplexUserAccount target = service.FindById(request.TargetAccount.Id);

            using (var context = new ComplexFriendRequestContext())
            {
                ComplexFriendRequest savedRequest = context.Objects.FirstOrDefault(req => req.Id == request.Id);

                if (savedRequest == null)
                {
                    var complexRequest = new ComplexFriendRequest
                    {
                        Id = request.Id,
                        ParentId = requester.Id,
                        TargetId = target.Id,
                        CreationDate = request.CreationDate,
                        RequestState = FriendRequestState.AwaitingUserAnswer
                    };

                    context.Objects.Add(complexRequest);

                    context.SaveChanges();

                    return complexRequest;
                }

                return savedRequest;
            }
        }

        /// <summary>
        /// Save friendrequest reply state.
        /// </summary>
        /// <param name="reply">User reply for a friend request.</param>
        public ComplexFriendRequest ReplyToRequest(FriendRequestReply reply) 
        {
            ComplexFriendRequest updatedRequest = SaveReplyState(reply);

            var interacter = AccountDbInteracter.Instance;
            var registrationService = RegistrationService.Instance;
            //var requesterId = reply.Request.RequesterAccount.UniqueId;
            var targetId = reply.Request.TargetAccount.Id;
            var requesterId = reply.Request.RequesterAccount.Id;

            var targetAcc = registrationService.FindById(targetId);
            var requesterAcc = registrationService.FindById(requesterId);
            if (targetAcc == null) throw new InvalidOperationException($"Target account '{ targetId }' not found");
            if (requesterAcc == null) throw new InvalidOperationException($"Requester account '{ requesterId }' not found");

            var friendship = new FriendshipModel
            {
                ParentId = reply.Request.RequesterAccount.Id,
                NavigationParent = reply.Request.RequesterAccount.ToComplex(),
                AcceptorAccountId = reply.Request.TargetAccount.Id,
                AcceptorAccount = reply.Request.TargetAccount.ToComplex(),
                Established = updatedRequest.DecisionDate,
            };

            FriendshipService.Instance.AddFriendship(friendship);

            targetAcc.Friendships.Add(friendship);
            requesterAcc.Friendships.Add(friendship);
            //interacter.AddOrUpdate(requesterAcc);
            //registrationService.UpdateCache(targetAcc);
            //registrationService.UpdateCache(requesterAcc);

            return updatedRequest;
        }

        /// <summary>
        /// Search for incoming friend requests by request target user identifier.
        /// </summary>
        /// <param name="requesterId">Target user identifier.</param>
        /// <returns>Incoming requests collection.</returns>
        public IEnumerable<ComplexFriendRequest> GetIncomingRequests(Guid requesterId) 
        {
            ValidateRequest(requesterId);

            bool Selector(ComplexFriendRequest request)
            {
                bool select = 
                    //request.TargetId == requesterId.ToString() && 
                    request.TargetId == requesterId && 
                    request.RequestState == FriendRequestState.AwaitingUserAnswer;

                return select;
            }

            return SearchRequests(Selector);
        }

        /// <summary>
        /// Search for outgoing friend requests by request sender user identifier.
        /// </summary>
        /// <param name="requesterId">Sender user identifier.</param>
        /// <returns>Outgoing requests collection.</returns>
        public IEnumerable<ComplexFriendRequest> GetOutgoingRequests(Guid requesterId) 
        {
            ValidateRequest(requesterId);

            //bool Selector(ComplexFriendRequest request) => request.ParentId == requesterId.ToString();
            bool Selector(ComplexFriendRequest request) => request.ParentId == requesterId;

            return SearchRequests(Selector);
        }


        private static void ValidateRequest(Guid requesterId) 
        {
            if (requesterId == Guid.Empty)
            {
                throw new InvalidOperationException("Requester id must not be empty guid");
            }
        }

        private static ComplexFriendRequest SaveReplyState(FriendRequestReply reply) 
        {
            using (var context = new ComplexFriendRequestContext())
            {
                var targetRequest = context.Objects.FirstOrDefault(request => request.Id == reply.Request.Id);

                if (targetRequest == null)
                {
                    throw new InvalidOperationException($"Friend request for reply with id {reply.Request.Id} not found");
                }

                if (reply.Reaction == FriendRequestReaction.Accept)
                {
                    targetRequest.RequestState = FriendRequestState.Accepted;
                }
                else if (reply.Reaction == FriendRequestReaction.Decline)
                {
                    targetRequest.RequestState = FriendRequestState.Declined;
                }
                else if (reply.Reaction == FriendRequestReaction.Ban)
                {
                    //TODO wut?
                }
                else
                {
                    // not ready yet
                    throw new NotImplementedException();
                }

                targetRequest.DecisionDate = DateTime.Now;

                context.SaveChanges();

                return targetRequest;
            }
        }

        private static IEnumerable<ComplexFriendRequest> SearchRequests(Func<ComplexFriendRequest, bool> selector) 
        {
            using (var context = new ComplexFriendRequestContext())
            {
                var result = 
                    context
                        .Objects
                        .Include(request => request.TargetAccount)
                        .Include(request => request.NavigationParent)
                        .Where(selector)
                        .ToList();

                return result;
            }
        }
    }
}