// Created by Laxale 01.12.2016
//
//


namespace Freengy.WebService 
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Server.Kestrel;

    public class Program 
    {
        public static void Main(string[] args)
        {
            string rootPath = Directory.GetCurrentDirectory();

            var hostingConfig =
                new ConfigurationBuilder()
                    .SetBasePath(rootPath)
                    .AddJsonFile("hosting.json", optional: true)
                    .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(hostingConfig) // can be used instead of harcoding URLs
                //.UseUrls("https://localhost:44000")
                //.UseWebListener()
                .UseKestrel
                (
                    opts =>
                    {
                        opts.NoDelay = false;
                        // configure with X509 sertificate
                        //opts.UseHttps()
                    }
                )
                .UseContentRoot(rootPath)
                //.UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}