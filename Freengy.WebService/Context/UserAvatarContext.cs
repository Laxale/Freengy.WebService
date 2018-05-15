// Created by Laxale 23.04.2018
//
//

using System.Data.Entity;

using Freengy.Common.Models.Avatar;
using Freengy.Database.Context;
using Freengy.WebService.Configuration;
using Freengy.WebService.Models;


namespace Freengy.WebService.Context 
{
    /// <summary>
    /// ORM context for <see cref="ComplexUserAvatarModel"/>.
    /// </summary>
    internal class UserAvatarContext : ComplexDbContext<ComplexUserAvatarModel> 
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder) 
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new UserAvatarConfiguration());

            CreateTable(modelBuilder);
        }
    }
}