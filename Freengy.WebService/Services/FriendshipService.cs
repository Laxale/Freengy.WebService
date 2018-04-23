// Created by Laxale 23.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Freengy.WebService.Context;
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
        private static readonly List<FriendshipModel> friendshipsCache = new List<FriendshipModel>();

        private static FriendshipService instance;

        
        private FriendshipService() 
        {

        }


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
                using (var dbContext = new FriendshipContext())
                {
                    var models =
                        dbContext
                            .Objects
                            .Include(model => model.AcceptorAccount)
                            .Include(model => model.NavigationParent)
                            .ToList();

                    foreach (FriendshipModel model in models)
                    {
                        friendshipsCache.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.Error(ex, $"Failed to initialize { nameof(FriendshipService) } service");
            }
        }

        /// <summary>
        /// Find friendship models by friendship acceptor identifier.
        /// </summary>
        /// <param name="acceptorId">Friendship acceptor identifier.</param>
        /// <returns>Collection of incoming to this account friendships.</returns>
        public IEnumerable<FriendshipModel> FindByAcceptor(Guid acceptorId) 
        {
            if(acceptorId == Guid.Empty) throw new InvalidOperationException("Acceptor id must not be empty");

            var cachedFriendship = friendshipsCache.Where(model => model.AcceptorAccountId == acceptorId);

            return cachedFriendship;
        }

        public void AddFriendship(FriendshipModel friendship) 
        {
            ValidateModel(friendship);

            using (var context = new FriendshipContext())
            {
                var existingModel =
                    context
                        .Objects
                        .Include(model => model.AcceptorAccount)
                        .Include(model => model.NavigationParent)
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

                friendshipsCache.Add(friendship);
            }
        }


        private void ValidateModel(FriendshipModel model) 
        {
            if(model == null) throw new ArgumentNullException(nameof(model));
            if(model.ParentId == Guid.Empty) throw new InvalidOperationException();
            if(model.AcceptorAccountId == Guid.Empty) throw new InvalidOperationException();

            model.PrepareMappedProps();
        }
    }
}