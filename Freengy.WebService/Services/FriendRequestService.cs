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
            ComplexUserAccount requester = service.FindById(request.RequesterAccount.UniqueId);
            ComplexUserAccount target = service.FindById(request.TargetAccount.UniqueId);

            using (var context = new ComplexFriendRequestContext())
            {
                ComplexFriendRequest savedRequest = context.Objects.FirstOrDefault(req => req.Id == request.Id);

                if (savedRequest == null)
                {
                    var complexRequest = new ComplexFriendRequest
                    {
                        Id = request.Id,
                        //RequesterAccount = requester,
                        //TargetAccount = target,
                        ParentId = requester.Id,
                        //RequesterId = requester.UniqueId,
                        TargetId = target.UniqueId.ToString(),
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
        /// Search for incoming friend requests by request target user identifier.
        /// </summary>
        /// <param name="requesterId">Target user identifier.</param>
        /// <returns>Incoming requests collection.</returns>
        public IEnumerable<ComplexFriendRequest> GetIncomingRequests(Guid requesterId)
        {
            ValidateRequest(requesterId);

            bool Selector(ComplexFriendRequest request) => request.TargetId == requesterId.ToString();

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

            bool Selector(ComplexFriendRequest request) => request.ParentId == requesterId.ToString();

            return SearchRequests(Selector);
        }


        private static void ValidateRequest(Guid requesterId) 
        {
            if (requesterId == Guid.Empty)
            {
                throw new InvalidOperationException("Requester id must not be empty guid");
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