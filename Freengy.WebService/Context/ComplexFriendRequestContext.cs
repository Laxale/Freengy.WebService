// Created by Laxale 19.04.2018
//
//

using System.Data.Entity;

using Freengy.Database.Context;
using Freengy.WebService.Configuration;
using Freengy.WebService.Models;


namespace Freengy.WebService.Context 
{
    /// <summary>
    /// ORM context for <see cref="ComplexFriendRequest"/>.
    /// </summary>
    internal class ComplexFriendRequestContext : ComplexDbContext<ComplexFriendRequest> 
    {
        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder) 
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new ComplexFriendRequestConfiguration());

            CreateTable(modelBuilder);
        }
    }
}