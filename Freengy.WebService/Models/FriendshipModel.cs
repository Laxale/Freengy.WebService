// Created by Laxale 19.04.2018
//
//

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Freengy.Common.Models;
using Freengy.Common.Database;


namespace Freengy.WebService.Models 
{
    /// <summary>
    /// Data model of a friendship relation.
    /// </summary>
    internal class FriendshipModel : ChildComplexDbObject<ComplexUserAccount> 
    {
        [Required]
        public Guid AcceptorAccountId { get; set; }

        /// <summary>
        /// Navigation property to get the acceptor friend account of this friendship.
        /// </summary>
        [ForeignKey(nameof(AcceptorAccountId))]
        public ComplexUserAccount AcceptorAccount { get; set; }

        /// <summary>
        /// Date of a friendship birthsday.
        /// </summary>
        public DateTime Established { get; set; }


        /// <inheritdoc />
        public override string ToString()
        {
            return $"Friendship from '{NavigationParent?.Name}' to '{AcceptorAccount?.Name}'";
        }

        public override DbObject CreateFromProxy(DbObject dbProxy) 
        {
            var proxyModel = (FriendshipModel) dbProxy;

            var realModel = new FriendshipModel
            {
                Id = proxyModel.Id,
                AcceptorAccountId = proxyModel.AcceptorAccountId,
                ParentId = proxyModel.ParentId,
                //AcceptorAccount = proxyModel.AcceptorAccount,
                //NavigationParent = proxyModel.NavigationParent,
                Established = proxyModel.Established
            };

            return realModel;
        }

        /// <summary>
        /// Need to clear navigation properties before storing to database.
        /// </summary>
        /// <returns>This instance prepared for storing to databse.</returns>
        public override ComplexDbObject PrepareMappedProps() 
        {
            AcceptorAccount = null;
            NavigationParent = null;

            return this;
        }

        protected override IEnumerable<string> GetIncludedPropNames() 
        {
            return new List<string>
            {
                //nameof(Friends)
            };
        }
    }
}