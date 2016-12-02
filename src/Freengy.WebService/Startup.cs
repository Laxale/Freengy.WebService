// Created by Laxale 01.12.2016
//
//


namespace Freengy.WebService 
{
    using System;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Freengy.WebService.Data;
    using Freengy.WebService.Models;
    using Freengy.WebService.Services;

    public class Startup 
    {
        public Startup(IHostingEnvironment env) 
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{ env.EnvironmentName }.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache(opts => opts.ExpirationScanFrequency = TimeSpan.FromSeconds(5));
            // Add framework services.
            this.AddMvcSsl(services);
            this.AddDatabase(services);
            this.AddSecurity(services);
            this.AddUserIdentity(services);
            this.AddInformingServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) 
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            
            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715
            var authOptions = new CookieAuthenticationOptions
            {
                AutomaticChallenge = true,
                AutomaticAuthenticate = true,
                AuthenticationScheme = "Coockies" // random name fro further authentication
            };

            //app.UseCookieAuthentication(authOptions);

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });
        }


        private void AddMvcSsl(IServiceCollection services) 
        {
            services.AddMvc(options =>
            {
                //options.SslPort = 44000;
                //options.Filters.Add(new RequireHttpsAttribute());
            });
        }
        private void AddDatabase(IServiceCollection services) 
        {
            Action<SqliteDbContextOptionsBuilder> sqliteBuilder =
                builder =>
                {
                    builder.CommandTimeout(2000);
                };

            services.AddDbContext<UsersDbContext>(optionsBilder =>
            {
                string connectionString = this.Configuration.GetConnectionString("DefaultConnection");

                optionsBilder.UseSqlite(connectionString, sqliteBuilder);
            });
        }
        private void AddSecurity(IServiceCollection services) 
        {
            services
                .AddAntiforgery()
                //.AddAuthorization()
                //.AddAuthentication()
                ;
        }
        private void AddUserIdentity(IServiceCollection services) 
        {
            services
                .AddIdentity<ApplicationUser, IdentityRole>(opts =>
                {
                    //opts.SignIn.RequireConfirmedEmail = true;
                })
                .AddEntityFrameworkStores<UsersDbContext>()
                .AddDefaultTokenProviders();
        }
        private void AddInformingServices(IServiceCollection services) 
        {
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<IEmailSender, AuthMessageSender>();
        }
    }
}