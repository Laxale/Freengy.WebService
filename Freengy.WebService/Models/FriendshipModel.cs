// Created by Laxale 19.04.2018
//
//

using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Friend account of this user relation.
        /// </summary>
        public ComplexUserAccount Account { get; set; }

        /// <summary>
        /// Date of a friendship birthsday.
        /// </summary>
        public DateTime Established { get; set; }

        
        public override DbObject CreateFromProxy(DbObject dbProxy) 
        {
            if (!(dbProxy is FriendshipModel model))
            {
                throw new InvalidOperationException("FFFUUUU");
            }

            return null;
        }

        public override ComplexDbObject PrepareMappedProps() 
        {
            //Friends.Clear();

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