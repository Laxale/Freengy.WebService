// Created by Laxale 19.04.2018
//
//

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        public string TargetId { get; set; }

        /// <summary>
        /// Navigation property for retrieving target user account of this request.
        /// </summary>
        [ForeignKey(nameof(TargetId))]
        public ComplexUserAccount TargetAccount { get; set; }

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
