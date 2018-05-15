using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freengy.Common.Database;
using Freengy.Common.Models;
using Freengy.Common.Models.Avatar;
using Freengy.WebService.Context;
using Freengy.WebService.Interfaces;
using Freengy.WebService.Models;


namespace Freengy.WebService.Services
{
    internal class ImageService : IService 
    {
        private static readonly object Locker = new object();

        private static ImageService instance;


        private ImageService() { }


        /// <summary>
        /// Единственный инстанс <see cref="ImageService"/>.
        /// </summary>
        public static ImageService Instance 
        {
            get
            {
                lock (Locker)
                {
                    return instance ?? (instance = new ImageService());
                }
            }
        }


        /// <summary>
        /// Initialize the service.
        /// </summary>
        public void Initialize() { }

        /// <summary>
        /// Get the avatar of target user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>User avatar model.</returns>
        public AvatarModel<ComplexUserAccount> GetUserAvatar(Guid userId) 
        {
            using (var context = new UserAvatarContext())
            {
                var targetAvatar = context.Objects.FirstOrDefault(avatarModel => avatarModel.ParentId == userId);

                return targetAvatar;
            }
        }

        /// <summary>
        /// Get the avatars of target users.
        /// </summary>
        /// <param name="userIds">Users identifiers collection.</param>
        /// <returns>User avatar models.</returns>
        public IEnumerable<AvatarModel<ComplexUserAccount>> GetUserAvatars(IEnumerable<Guid> userIds) 
        {
            using (var context = new UserAvatarContext())
            {
                var targetAvatars = context.Objects.Where(avatarModel => userIds.Contains(avatarModel.ParentId)).ToList();

                return targetAvatars;
            }
        }

        /// <summary>
        /// Get the user avatars cache information.
        /// </summary>
        /// <param name="userIds">Users identifiers collection.</param>
        /// <returns>User avatars cache information.</returns>
        public IEnumerable<ObjectModificationTime> GetUserAvatarsCache(IEnumerable<Guid> userIds) 
        {
            using (var context = new UserAvatarContext())
            {
                var targetAvatars = context.Objects.Where(avatarModel => userIds.Contains(avatarModel.ParentId));
                var cacheInfos = 
                    targetAvatars
                        .Select(avatar => 
                            new ObjectModificationTime
                            {
                                ObjectId = avatar.ParentId, ModificationTime = avatar.LastModified
                            })
                        .ToList();

                return cacheInfos;
            }
        }
    }
}