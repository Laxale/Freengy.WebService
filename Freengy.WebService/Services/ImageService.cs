using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freengy.Common.Database;
using Freengy.Common.Models.Avatar;
using Freengy.WebService.Interfaces;

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
        /// Get the avatar of some parent (avatarable) entity.
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        public AvatarModel<TParent> GetAvatar<TParent>(Guid avatarId) where TParent : ComplexDbObject, new () 
        {
            throw new NotImplementedException();
        }
    }
}