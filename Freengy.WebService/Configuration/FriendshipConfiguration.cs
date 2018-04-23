// Created by Laxale 19.04.2018
//
//

using System.Data.Entity.ModelConfiguration;
using System.Linq;

using Freengy.WebService.Models;


namespace Freengy.WebService.Configuration 
{
    /// <summary>
    /// ORM configuration for <see cref="FriendshipModel"/>.
    /// </summary>
    internal class FriendshipConfiguration : EntityTypeConfiguration<FriendshipModel> 
    {
        public FriendshipConfiguration() 
        {
            ToTable($"{nameof(FriendshipModel)}s");

            HasRequired(friendship => friendship.NavigationParent)
                .WithMany(acc => acc.Friendships)
                .HasForeignKey(friendship => friendship.ParentId)
                .WillCascadeOnDelete(true);
        }
    }
}