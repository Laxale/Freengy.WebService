// Created by Laxale 18.04.2018
//
//


using System;
using System.Linq;

using Freengy.Common.Models;
using Freengy.Database.Context;
using Freengy.WebService.Context;
using Freengy.WebService.Interfaces;
using Freengy.WebService.Models;

using NLog;


namespace Freengy.WebService.Services 
{
    /// <summary>
    /// Interacts user accounts data with database.
    /// </summary>
    internal class AccountDbInteracter : IService 
    {
        private static readonly object Locker = new object();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static AccountDbInteracter instance;


        private AccountDbInteracter() 
        {

        }


        /// <summary>
        /// Единственный инстанс <see cref="AccountDbInteracter"/>.
        /// </summary>
        public static AccountDbInteracter Instance 
        {
            get
            {
                lock (Locker)
                {
                    return instance ?? (instance = new AccountDbInteracter());
                }
            }
        }


        /// <inheritdoc />
        /// <summary>
        /// Initialize the service.
        /// </summary>
        public void Initialize() 
        {
            try
            {
                using (var dbContext = new ComplexUserContext())
                {
                    ComplexUserAccount storedAcc = dbContext.Objects.FirstOrDefault();

                    var test = storedAcc?.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.Error(ex, $"Failed to initialize { nameof(AccountDbInteracter) } service");
            }
        }

        /// <summary>
        /// Add new or update already registered account in database.
        /// </summary>
        /// <param name="account">Account to add or update.</param>
        public void AddOrUpdate(ComplexUserAccount account) 
        {
            lock (Locker)
            {
                try
                {
                    using (var dbContext = new ComplexUserContext())
                    {
                        var storedAcc = dbContext.Objects.FirstOrDefault(acc => acc.Id == account.Id);
                        if (storedAcc == null)
                        {
                            dbContext.Objects.Add(account);
                            Console.WriteLine($"Added new account '{ account.Name }'");
                        }
                        else
                        {
                            storedAcc.SyncUniqueIdToId();

                            TransferProperties(storedAcc, account);

                            Console.WriteLine($"Updated account '{ account.Name }'");
                        }

                        dbContext.SaveChanges();
                        Console.WriteLine("Saved account changes");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    logger.Error(ex, $"Failed to add or update account '{ account.Name }'");
                }
            }
        }


        private void TransferProperties(ComplexUserAccount inDb, ComplexUserAccount incoming) 
        {
            if(inDb.Id != incoming.Id) throw new InvalidOperationException("Unique id mismatch");
            if(inDb.UniqueId != incoming.UniqueId) throw new InvalidOperationException("Unique id mismatch");

            inDb.Name = incoming.Name;
            inDb.Level = incoming.Level;
            inDb.Privilege = incoming.Privilege;
            inDb.LastLogInTime = incoming.LastLogInTime;
            inDb.Friendships = incoming.Friendships;
        }
    }
}