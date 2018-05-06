// Created by Laxale 06.05.2018
//
//

using System.Data.Entity.ModelConfiguration;

using Freengy.WebService.Models;


namespace Freengy.WebService.Configuration 
{
    /// <summary>
    /// EF-configuration for working with <see cref="Password"/> models.
    /// </summary>
    internal class PasswordConfiguration : EntityTypeConfiguration<Password> 
    {
        public PasswordConfiguration() 
        {
            ToTable($"{nameof(Password)}s");

            HasRequired(password => password.NavigationParent)
                .WithMany(acc => acc.PasswordDatas)
                .HasForeignKey(pass => pass.ParentId)
//                // в связи с дикими проблемами наладить отношение один-к-одному пришлось вхерачить коллекцию
//                // в этой коллекции всегда один пароль
                  //.WithRequiredDependent(account => account.PasswordData)
                .WillCascadeOnDelete(true);
        }
    }
}