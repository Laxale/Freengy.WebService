// Created by Laxale 17.04.2018
//
//

using System;

using Nancy;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;


namespace Freengy.WebService 
{
    internal static class Program 
    {
        private static readonly int httpsPort = 22345;
        private static readonly string murkCertHash = "input cert thumb here";
        private static readonly string appPid = $"{{{ Guid.NewGuid() }}}";
        private static readonly string httpAddress = "http://localhost:12345";
        private static readonly string httpsAddress = $"https://localhost:{ httpsPort }";
        //private static readonly string httpsAddress = $"https://127.0.0.1:{ httpsPort }";
        //private static readonly string httpsAddress = $"https://www.murk.pro:{ httpsPort }";


        static void Main(string[] args) 
        {
            try
            {
                Console.WriteLine("Starting Freengy server");

                StartServer();

                Console.WriteLine("Finished Freengy server");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("Press any key to finish");
                Console.ReadKey();
            }
        }


        static void StartServer() 
        {
            var baseUri = new Uri(httpAddress);
            //var baseUri = new Uri(httpsAddress);
            INancyBootstrapper booter = CreateBootstrapper();
            HostConfiguration configuration = CreateConfiguration();

            using (var host = new NancyHost(booter, configuration, baseUri))
            {
                EnableSsl();
                host.Start();

                Console.WriteLine($"Started host on '{ baseUri.AbsoluteUri }'");
                Console.WriteLine("Press Esc to stop");

                var keyInfo = Console.ReadKey();
                while (keyInfo.Key != ConsoleKey.Escape)
                {
                    keyInfo = Console.ReadKey();
                }
            }
        }

        static HostConfiguration CreateConfiguration() 
        {
            var config = new HostConfiguration
            {
                EnableClientCertificates = true,
                UrlReservations = new UrlReservations
                {
                    CreateAutomatically = true
                }
            };

            config.UnhandledExceptionCallback = ex =>
            {
                Console.WriteLine($"Unhandled ex: { ex.Message }");
            };

            return config;
        }

        static INancyBootstrapper CreateBootstrapper() 
        {
            return new DefaultNancyBootstrapper();
        }

        static void EnableSsl() 
        {
            string command = $"netsh http add sslcert ipport=0.0.0.0:{ httpsPort } certhash={ murkCertHash } appid={ appPid } clientcertnegotiation=enable";
            //var commandBytes = Encoding.UTF8.GetBytes(command);

            //var currentProc = Process.GetCurrentProcess();

            //currentProc.StandardInput.Write(command);
            //input.Write(commandBytes, 0, commandBytes.Length);
        }
    }
}