// Created by Laxale 06.05.2018
//
//

using System.Data.Entity.ModelConfiguration;

using Freengy.Common.Models.Avatar;
using Freengy.WebService.Models;


namespace Freengy.WebService.Configuration 
{
    /// <summary>
    /// EF-configuration for working with <see cref="ComplexUserAvatarModel"/> models.
    /// </summary>
    internal class UserAvatarConfiguration : EntityTypeConfiguration<ComplexUserAvatarModel> 
    {
        public UserAvatarConfiguration() 
        {
            ToTable("UserAvatars");

            HasRequired(avatar => avatar.NavigationParent)
                .WithMany(acc => acc.Avatars)
                .HasForeignKey(pass => pass.ParentId)
//                // в связи с дикими проблемами наладить отношение один-к-одному пришлось вхерачить коллекцию
//                // в этой коллекции всегда один аватар или пусто
                .WillCascadeOnDelete(true);
        }
    }
}