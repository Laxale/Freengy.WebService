// Created by Laxale 23.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Freengy.WebService.Context;
using Freengy.WebService.Helpers;
using Freengy.WebService.Interfaces;
using Freengy.WebService.Models;

using NLog;


namespace Freengy.WebService.Services 
{
    /// <summary>
    /// Service to handle friendships.
    /// </summary>
    internal class FriendshipService : IService 
    {
        private static readonly object Locker = new object();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<Guid, List<FriendshipModel>> outgoingFriendships = new Dictionary<Guid, List<FriendshipModel>>();
        private static readonly Dictionary<Guid, List<FriendshipModel>> incomingFriendships = new Dictionary<Guid, List<FriendshipModel>>();

        private static FriendshipService instance;

        
        private FriendshipService() { }


        /// <summary>
        /// Единственный инстанс <see cref="FriendshipService"/>.
        /// </summary>
        public static FriendshipService Instance 
        {
            get
            {
                lock (Locker)
                {
                    return instance ?? (instance = new FriendshipService());
                }
            }
        }


        /// <summary>
        /// Initialize the service.
        /// </summary>
        public void Initialize() 
        {
            try
            {
                using (var context = new FriendshipContext())
                {
                    var allFriendships = context.Objects.ToList();

                    foreach (var friendshipProxy in allFriendships)
                    {
                        var realFriendship = (FriendshipModel)friendshipProxy.CreateFromProxy(friendshipProxy);

                        CacheOutgoingFriendship(realFriendship);
                        CacheIncomingFriendship(realFriendship);
                    }
                }

                $"Cached {incomingFriendships.Count} friendships".WriteToConsole();
                logger.Info($"Initialized { nameof(FriendshipService) }");
            }
            catch (Exception ex)
            {
                string message = $"Failed to initialize {nameof(FriendshipService)}";
                message.WriteToConsole();
                logger.Error(ex, message);
            }
        }

        /// <summary>
        /// Find friendship models by friendship acceptor identifier.
        /// </summary>
        /// <param name="acceptorId">Friendship acceptor identifier.</param>
        /// <returns>Collection of incoming to this account friendships.</returns>
        public IEnumerable<FriendshipModel> FindByAcceptor(Guid acceptorId) 
        {
            if(acceptorId == Guid.Empty) throw new InvalidOperationException("Acceptor account id must not be empty");

            List<FriendshipModel> cachedFriendships =
                incomingFriendships.ContainsKey(acceptorId) ?
                    incomingFriendships[acceptorId] :
                    new List<FriendshipModel>();

            return cachedFriendships;
        }

        /// <summary>
        /// Find friendship models by friendship invocator identifier.
        /// </summary>
        /// <param name="invokerId"></param>
        /// <returns></returns>
        public IEnumerable<FriendshipModel> FindByInvoker(Guid invokerId) 
        {
            if (invokerId == Guid.Empty) throw new InvalidOperationException("Invoker account id must not be empty");

            List<FriendshipModel> cachedFriendships =
                outgoingFriendships.ContainsKey(invokerId) ? 
                    outgoingFriendships[invokerId] : 
                    new List<FriendshipModel>();

            return cachedFriendships;
        }

        /// <summary>
        /// Save the friendship to database.
        /// </summary>
        /// <param name="friendship">Friendship model.</param>
        public void SaveFriendship(FriendshipModel friendship) 
        {
            ValidateModel(friendship);

            using (var context = new FriendshipContext())
            {
                var existingModel =
                    context
                        .Objects
                        .FirstOrDefault(model =>
                            model.ParentId == friendship.ParentId &&
                            model.AcceptorAccountId == friendship.AcceptorAccountId);

                if (existingModel == null)
                {
                    context.Objects.Add(friendship);
                }
                else
                {
                    string message = $"Friendship between {existingModel.NavigationParent.Name} and { existingModel.AcceptorAccount.Name } already exists";
                    throw new InvalidOperationException(message);
                }

                context.SaveChanges();

                CacheIncomingFriendship(friendship);
                CacheOutgoingFriendship(friendship);
            }
        }

        /// <summary>
        /// Clear and refill account friendships with actual data.
        /// </summary>
        /// <param name="account"></param>
        public void UpdateFriendships(ComplexUserAccount account) 
        {
            IEnumerable<FriendshipModel> outFriendships = FindByInvoker(account.Id);
            IEnumerable<FriendshipModel> inFriendships = FindByAcceptor(account.Id);
            IEnumerable<FriendshipModel> allAccountFriendships = outFriendships.Union(inFriendships, new FriendshipComparer());

            account.Friendships.Clear();
            account.Friendships.AddRange(allAccountFriendships);
        }


        private void ValidateModel(FriendshipModel model) 
        {
            if(model == null) throw new ArgumentNullException(nameof(model));
            if(model.ParentId == Guid.Empty) throw new InvalidOperationException();
            if(model.AcceptorAccountId == Guid.Empty) throw new InvalidOperationException();

            model.PrepareMappedProps();
        }

        private static void CacheIncomingFriendship(FriendshipModel realFriendship) 
        {
            CacheFriendship(incomingFriendships, realFriendship, realFriendship.AcceptorAccountId);
        }

        private static void CacheOutgoingFriendship(FriendshipModel realFriendship) 
        {
            CacheFriendship(outgoingFriendships, realFriendship, realFriendship.ParentId);
        }

        private static void CacheFriendship(Dictionary<Guid, List<FriendshipModel>> cache, FriendshipModel realFriendship, Guid userId) 
        {
            if (!cache.ContainsKey(userId))
            {
                cache.Add(userId, new List<FriendshipModel>());
            }

            var friendships = cache[userId];
            if (!friendships.Contains(realFriendship))
            {
                friendships.Add(realFriendship);
            }
        }
    }
}