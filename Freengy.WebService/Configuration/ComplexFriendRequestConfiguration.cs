// Created by Laxale 19.04.2018
//
//

using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

using Freengy.Common.Models;
using Freengy.WebService.Models;


namespace Freengy.WebService.Configuration 
{
    /// <inheritdoc />
    /// <summary>
    /// ORM configuration for a <see cref="ComplexFriendRequest"/>.
    /// </summary>
    internal class ComplexFriendRequestConfiguration : EntityTypeConfiguration<ComplexFriendRequest> 
    {
        public ComplexFriendRequestConfiguration() 
        {
            ToTable($"{nameof(FriendRequest)}s");

            HasRequired(request => request.NavigationParent)
                .WithMany(acc => acc.FriendRequests)
                .HasForeignKey(request => request.ParentId)
                .WillCascadeOnDelete(true);
        }
    }
}