// Created by Laxale 19.04.2018
//
//

using System;
using System.Collections.Generic;
using Freengy.Common.Database;
using Freengy.Common.Models;
using Freengy.WebService.Context;
using Freengy.WebService.Extensions;


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
        public List<FriendshipModel> Friendships { get; set; } = new List<FriendshipModel>();


        public override DbObject CreateFromProxy(DbObject dbProxy) 
        {
            if (dbProxy is ComplexUserAccount account)
            {
                ComplexUserAccount fromProxy = account.Copy();

                return fromProxy;
            }

            throw new InvalidOperationException("Not an account proxy!");
        }

        protected override List<string> GetIncludedPropNames() 
        {
            return new List<string>
            {
                nameof(Friendships)
            };
        }

        public override ComplexDbObject PrepareMappedProps()
        {
            //nothin
            return this;
        }
    }
}