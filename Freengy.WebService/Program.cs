// Created by Laxale 17.04.2018
//
//

using System;

using Freengy.Common.Constants;
using Freengy.Common.Helpers;
using Freengy.Database;
using Freengy.WebService.Constants;
using Freengy.WebService.Helpers;
using Freengy.WebService.Services;
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

        //private static readonly string httpAddressNoPort = "192.168.213.146";
        private static readonly string httpAddressNoPort = "localhost";
        //private static readonly string httpAddress = "http://localhost:12345";
        //private static readonly string httpsAddress = $"https://localhost:{ httpsPort }";
        //private static readonly string httpsAddress = $"https://127.0.0.1:{ httpsPort }";
        //private static readonly string httpsAddress = $"https://www.murk.pro:{ httpsPort }";


        private static void Main(string[] args) 
        {
            try
            {
                "App started".WriteToConsole();

                InitDatabase();
                InitServices();
                StartServer();
            }
            catch (Exception ex)
            {
                ex.Message.WriteToConsole(ConsoleColor.Red);
            }
            finally
            {
                "Press any key to finish".WriteToConsole();
                Console.ReadKey();
            }
        }


        private static void InitDatabase() 
        {
            string appDataFolderPath = Initializer.GetFolderPathInAppData(FreengyPaths.AppDataRootFolderName);
            Initializer.SetStorageDirectoryPath(appDataFolderPath);
            Initializer.SetDbFileName(ServiceDbConst.DbFileName);

            "Initialized database".WriteToConsole();
        }

        private static void InitServices() 
        {
            new ServicesInitializer()
                .Register(AccountDbInteracter.Instance)
                .Register(AccountStateService.Instance)
                .Register(FriendRequestService.Instance)
                .Register(FriendshipService.Instance)
                .Register(RegistrationService.Instance)
                .InitRegistered();

            "Initialized services".WriteToConsole();
        }

        private static void StartServer() 
        {
            string address =
                new ServerStartupBuilder()
                .SetBaseAddress(httpAddressNoPort)
                .SetInitialPort(StartupConst.InitialServerPort)
                .SetPortStep(StartupConst.PortCheckingStep)
                .SetTrialsCount(10)
                .UseHttps(false)
                .Build();

            $"Started server on { address }".WriteToConsole();
            "Press Esc to exit".WriteToConsole();

            var keyInfo = Console.ReadKey();
            while (keyInfo.Key != ConsoleKey.Escape)
            {
                keyInfo = Console.ReadKey();
            }

            "Finished Freengy server".WriteToConsole();
        }

        private static HostConfiguration CreateConfiguration() 
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
                $"Unhandled ex: { ex.Message }".WriteToConsole(ConsoleColor.Red);
            };

            return config;
        }

        private static INancyBootstrapper CreateBootstrapper() 
        {
            return new DefaultNancyBootstrapper();
        }

        private static void EnableSsl() 
        {
            string command = $"netsh http add sslcert ipport=0.0.0.0:{ httpsPort } certhash={ murkCertHash } appid={ appPid } clientcertnegotiation=enable";
            //var commandBytes = Encoding.UTF8.GetBytes(command);

            //var currentProc = Process.GetCurrentProcess();

            //currentProc.StandardInput.Write(command);
            //input.Write(commandBytes, 0, commandBytes.Length);
        }
    }
}