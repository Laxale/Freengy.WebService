// Created by Laxale 23.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;

using Freengy.Common.Constants;
using Freengy.Common.Enums;
using Freengy.Common.Extensions;
using Freengy.Common.Helpers;
using Freengy.Common.Models;
using Freengy.Common.Models.Avatar;
using Freengy.WebService.Extensions;
using Freengy.WebService.Models;
using Freengy.WebService.Services;

using Nancy;
using Nancy.Responses;


namespace Freengy.WebService.Modules 
{
    /// <summary>
    /// Module for handling user avatars search.
    /// </summary>
    public class AvatarModule : NancyModule 
    {
        public AvatarModule() 
        {
            Post[Subroutes.Search.SearchUserAvatars] = OnSearhUserAvatars;
        }


        private dynamic OnSearhUserAvatars(dynamic arg) 
        {
            var searchRequest = new SerializeHelper().DeserializeObject<SearchRequest>(Request.Body);

            var imageService = ImageService.Instance;
            var stateService = AccountStateService.Instance;

            Guid senderId = Request.Headers.GetClientId();
            SessionAuth clientAuth = Request.Headers.GetSessionAuth();
            if (!stateService.IsAuthorized(senderId, clientAuth.ClientToken))
            {
                return HttpStatusCode.Forbidden;
            }

            if (string.IsNullOrWhiteSpace(searchRequest.SearchFilter))
            {
                return HttpStatusCode.BadRequest;
            }

            var responceData = new UserAvatarsReply();
            IEnumerable<Guid> userIds = searchRequest.SearchFilter.Split(';').Select(Guid.Parse).ToList();
            if (searchRequest.Entity == SearchEntity.UserAvatars)
            {
                var avatars = 
                    imageService
                        .GetUserAvatars(userIds)
                        .Select(complexAvatar => complexAvatar.ToSimple())
                        .ToList();

                responceData.UserAvatars = avatars;
            }
            else if (searchRequest.Entity == SearchEntity.UserAvatarsCache)
            {
                List<ObjectModificationTime> avatarsCache = imageService.GetUserAvatarsCache(userIds).ToList();

                responceData.AvatarsModifications = avatarsCache;
            }

            var responce = new JsonResponse<UserAvatarsReply>(responceData, new DefaultJsonSerializer());
            responce.Headers.Add(FreengyHeaders.Server.ServerSessionTokenHeaderName, clientAuth.ServerToken);

            return responce;
        }
    }
}