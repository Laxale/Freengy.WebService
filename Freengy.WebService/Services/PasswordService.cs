// Created by Laxale 07.05.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Freengy.Common.Helpers;
using Freengy.WebService.Context;
using Freengy.WebService.Helpers;
using Freengy.WebService.Interfaces;
using Freengy.WebService.Models;

using NLog;

using SecurityDriven.Inferno;
using SecurityDriven.Inferno.Kdf;
using SecurityDriven.Inferno.Mac;
using SecurityDriven.Inferno.Extensions;


namespace Freengy.WebService.Services 
{
    internal class PasswordService : IService 
    {
        private static readonly int hashBytesCount = 48;
        private static readonly int iterationCount = 100000;
        private static readonly object Locker = new object();
        private static readonly Func<HMAC> sha384Func = HMACFactories.HMACSHA384;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static PasswordService instance;

        /// <summary>
        /// Key is account id, value is account password.
        /// </summary>
        private readonly Dictionary<Guid, Password> registeredPasswords = new Dictionary<Guid, Password>();


        private PasswordService() { }


        /// <summary>
        /// Единственный инстанс <see cref="PasswordService"/>.
        /// </summary>
        public static PasswordService Instance 
        {
            get
            {
                lock (Locker)
                {
                    return instance ?? (instance = new PasswordService());
                }
            }
        }


        /// <summary>
        /// Initialize the service.
        /// </summary>
        public void Initialize() 
        {
            try
            {
                using (var context = new PasswordContext())
                {
                    var allPasswords = context.Objects.ToList();

                    foreach (var password in allPasswords)
                    {
                        var realPassword = (Password)password.CreateFromProxy(password);

                        registeredPasswords.Add(realPassword.ParentId, realPassword);
                    }
                }

                $"Cached {registeredPasswords.Count} passwords".WriteToConsole();
                logger.Info($"Initialized { nameof(PasswordService) }");
            }
            catch (Exception ex)
            {
                string message = $"Failed to initialize {nameof(PasswordService)}";
                message.WriteToConsole();
                logger.Error(ex, message);
            }
        }

        public void SaveOrUpdatePassword(Password password, bool addToDatabase) 
        {
            if (registeredPasswords.ContainsKey(password.ParentId))
            {
                registeredPasswords[password.ParentId] = password;
            }
            else
            {
                registeredPasswords.Add(password.ParentId, password);
            }
            
            if (addToDatabase)
            {
                using (var context = new PasswordContext())
                {
                    var existingPass = context.Objects.FirstOrDefault(pass => pass.Id == password.Id);

                    if (existingPass != null)
                    {
                        context.Objects.Remove(existingPass);
                    }

                    context.Objects.Add(password);

                    context.SaveChanges();
                }
            }
        }

        public Password GetPasswordOf(Guid userId) 
        {
            try
            {
                return registeredPasswords[userId];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public bool ValidatePassword(Guid userId, string password) 
        {
            Password targetPassword = registeredPasswords[userId];

            if (targetPassword == null)
            {
                throw new InvalidOperationException($"Password for user id '{ userId }' is not registered");
            }

            byte[] saltBytes = targetPassword.NextLoginSalt.FromB64();
            byte[] storedHashBytes = targetPassword.NextPasswordHash.FromB64();
            byte[] passedHashBytes =
                new PBKDF2(sha384Func, password, saltBytes, iterationCount)
                    .GetBytes(hashBytesCount);

            bool areEqual = Utils.ConstantTimeEqual(storedHashBytes, passedHashBytes);

            return areEqual;
        }

        public Password CreatePasswordData(string password) 
        {
            byte[] saltBytes = new byte[hashBytesCount];
            new CryptoRandom().NextBytes(saltBytes);
            string nextSalt = saltBytes.ToB64();
            byte[] hashBytes = 
                new PBKDF2(sha384Func, password, saltBytes, iterationCount)
                .GetBytes(hashBytesCount);

            var passwordData = new Password
            {
                NextLoginSalt = nextSalt,
                NextPasswordHash = hashBytes.ToB64()
            };

            return passwordData;
        }
    }
}