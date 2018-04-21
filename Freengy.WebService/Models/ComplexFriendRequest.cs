// Created by Laxale 19.04.2018
//
//

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Freengy.Common.Models;
using Freengy.Common.Enums;
using Freengy.Common.Database;


namespace Freengy.WebService.Models 
{
    internal class ComplexFriendRequest : ChildComplexDbObject<ComplexUserAccount> 
    {
        /// <summary>
        /// The identifier of a request target user.
        /// </summary>
        public string TargetId { get; set; }

        [NotMapped]
        public ComplexUserAccount TargetAccount { get; set; }

        /// <summary>
        /// The identifier of a request sender user.
        /// </summary>
        //[NotMapped]
        //public Guid RequesterId { get; set; }

        //[NotMapped]
        //public ComplexUserAccount RequesterAccount { get; set; }

        /// <summary>
        /// Request creation timestamp.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Request decision timestamp.
        /// </summary>
        public DateTime DecisionDate { get; set; }

        /// <summary>
        /// State of this request.
        /// </summary>
        public FriendRequestState RequestState { get; set; }


        /// <inheritdoc />
        public override DbObject CreateFromProxy(DbObject dbProxy)
        {
            return (ComplexFriendRequest)dbProxy;
        }

        /// <inheritdoc />
        public override ComplexDbObject PrepareMappedProps() 
        {
            return this;
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetIncludedPropNames() 
        {
            return new List<string>();
        }
    }
}
