// Created by Laxale 01.12.2016
//
//


namespace Freengy.WebService.Data 
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    
    using Freengy.WebService.Models;
    
    public class UsersDbContext : IdentityDbContext<ApplicationUser> 
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) 
        {

        }

        protected override void OnModelCreating(ModelBuilder builder) 
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}