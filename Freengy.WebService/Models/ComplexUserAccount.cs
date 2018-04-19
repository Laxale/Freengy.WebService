// Created by Laxale 19.04.2018
//
//

using System;
using System.Collections.Generic;

using Freengy.Common.Database;
using Freengy.Common.Models;
using Freengy.WebService.Context;
using Freengy.WebService.Extensions;

using Newtonsoft.Json;


namespace Freengy.WebService.Models 
{
    /// <summary>
    /// Service-wide user account model with non-client database relations.
    /// </summary>
    internal class ComplexUserAccount : UserAccount 
    {
        /// <summary>
        /// Friendship relation models of this user account.
        /// </summary>
        [JsonIgnore]
        public List<FriendshipModel> Friendships { get; set; } = new List<FriendshipModel>();

        /// <summary>
        /// Friend request models of this user account.
        /// </summary>
        [JsonIgnore]
        public List<ComplexFriendRequest> FriendRequests { get; set; } = new List<ComplexFriendRequest>();


        /// <inheritdoc />
        public override string ToString() 
        {
            return $"{Name} [Level {Level} {Privilege}]";
        }

        public override DbObject CreateFromProxy(DbObject dbProxy) 
        {
            if (dbProxy is ComplexUserAccount account)
            {
                ComplexUserAccount fromProxy = account.Copy();

                return fromProxy;
            }

            throw new InvalidOperationException("Not an account proxy!");
        }

        public override ComplexDbObject PrepareMappedProps()
        {
            //nothin
            return this;
        }


        protected override IEnumerable<string> GetIncludedPropNames() 
        {
            return new List<string>
            {
                nameof(Friendships)
            };
        }
    }
}