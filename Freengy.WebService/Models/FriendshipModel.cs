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
        public FriendshipModel()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("created friendship");
            Console.ForegroundColor = ConsoleColor.White;
        }

        [Required]
        public string AcceptorAccountId { get; set; }

        /// <summary>
        /// Navigation property to get the acceptor friend account of this friendship.
        /// </summary>
        [ForeignKey(nameof(AcceptorAccountId))]
        public ComplexUserAccount AcceptorAccount
        {
            get => acceptorAccount;
            set
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("ser value");
                acceptorAccount = value;
            }

        }

        private ComplexUserAccount acceptorAccount;

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