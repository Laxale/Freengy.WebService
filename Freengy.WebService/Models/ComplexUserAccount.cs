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
    internal class ComplexUserAccount : UserAccountModel 
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
                account.Friendships.ForEach(fromProxy.Friendships.Add);
                account.FriendRequests.ForEach(fromProxy.FriendRequests.Add);

                return fromProxy;
            }

            throw new InvalidOperationException("Not an account proxy!");
        }

        /// <summary>
        /// Need to clear navigation properties before storing to database.
        /// </summary>
        /// <returns>This instance prepared for storing to databse.</returns>
        public override ComplexDbObject PrepareMappedProps() 
        {
            Friendships.Clear();
            FriendRequests.Clear();

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