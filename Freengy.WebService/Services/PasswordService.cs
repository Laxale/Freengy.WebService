// Created by Laxale 07.05.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using Freengy.Common.Helpers;
using Freengy.WebService.Context;
using Freengy.WebService.Helpers;
using Freengy.WebService.Interfaces;
using Freengy.WebService.Models;

using NLog;


namespace Freengy.WebService.Services 
{
    internal class PasswordService : IService 
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly object Locker = new object();

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

        public void SavePassword(Password password, bool addToDatabase) 
        {
            if (registeredPasswords.ContainsKey(password.ParentId))
            {
                throw new InvalidOperationException($"Password of account '{ password.ParentId }' already cahced");
            }

            registeredPasswords.Add(password.ParentId, password);

            if (addToDatabase)
            {
                using (var context = new PasswordContext())
                {
                    var existingPass = context.Objects.FirstOrDefault(pass => pass.Id == password.Id);

                    if (existingPass != null)
                    {
                        throw new InvalidOperationException($"Password of account '{ password.ParentId }' already stored in db");
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

        public bool ValidatePassword(Guid userId, string saltedPasswordHash) 
        {
            Password targetPassword = registeredPasswords[userId];

            if (targetPassword == null)
            {
                throw new InvalidOperationException($"Password for user id '{ userId }' is not registered");
            }

            var hasher = new Hasher();
            var doubleHash = hasher.GetHash(targetPassword.NextLoginSalt + saltedPasswordHash);

            bool areEqual = doubleHash == targetPassword.NextPasswordHash;

            return areEqual;
        }

        public Password CreatePasswordData(string password) 
        {
            var hasher = new Hasher();
            string nextSalt = hasher.GetHash(Guid.NewGuid().ToString());
            string passwordHash = hasher.GetHash(nextSalt + password);
            string nextTotalHash = hasher.GetHash(nextSalt + passwordHash);

            var passwordData = new Password
            {
                NextLoginSalt = nextSalt,
                NextPasswordHash = nextTotalHash
            };

            return passwordData;
        }
    }
}