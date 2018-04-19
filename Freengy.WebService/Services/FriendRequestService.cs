// Created by Laxale 19.04.2018
//
//

using Freengy.Common.Models;


namespace Freengy.WebService.Services 
{
    using System.Linq;

    using Freengy.Common.Enums;
    using Freengy.Database.Context;
    using Freengy.WebService.Context;
    using Freengy.WebService.Models;


    internal class FriendRequestService 
    {
        private static readonly object Locker = new object();

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
                        RequesterAccount = requester,
                        TargetAccount = target,
                        ParentId = requester.Id,
                        RequesterId = requester.UniqueId,
                        TargetId = target.UniqueId,
                        CreationDate = request.CreationDate
                    };

                    context.Objects.Add(complexRequest);

                    context.SaveChanges();

                    return complexRequest;
                }

                return savedRequest;
            }
        }
    }
}