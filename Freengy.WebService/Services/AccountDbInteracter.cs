﻿// Created by Laxale 18.04.2018
//
//


using System;
using System.Linq;

using Freengy.Common.Models;
using Freengy.Database.Context;
using Freengy.WebService.Context;
using Freengy.WebService.Helpers;
using Freengy.WebService.Interfaces;
using Freengy.WebService.Models;

using NLog;


namespace Freengy.WebService.Services 
{
    using System.Data.Entity;


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
                ex.Message.WriteToConsole(ConsoleColor.Red);
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
                        var storedAcc = 
                            dbContext
                                .Objects
                                //.Include(acc => acc.Friendships)
                                //.Include(acc => acc.FriendRequests)
                                .FirstOrDefault(acc => acc.Id == account.Id);
                        if (storedAcc == null)
                        {
                            dbContext.Objects.Add(account);
                            $"Added new account '{ account.Name }'".WriteToConsole();
                        }
                        else
                        {
                            TransferProperties(account, storedAcc);
                            storedAcc.PrepareMappedProps();

                            $"Updated account '{ account.Name }'".WriteToConsole();
                        }

                        dbContext.SaveChanges();
                        "Saved account changes".WriteToConsole();
                    }
                }
                catch (Exception ex)
                {
                    ex.Message.WriteToConsole(ConsoleColor.Red);
                    logger.Error(ex, $"Failed to add or update account '{ account.Name }'");
                }
            }
        }


        public static void TransferProperties(ComplexUserAccount from, ComplexUserAccount to) 
        {
            if(to.Id != from.Id) throw new InvalidOperationException("Id mismatch");
            //if(to.UniqueId != from.UniqueId) throw new InvalidOperationException("Unique id mismatch");

            to.Name = from.Name;
            to.Level = from.Level;
            to.Privilege = from.Privilege;
            to.LastLogInTime = from.LastLogInTime;

            //to.Friendships = from.Friendships;
            to.Friendships.Clear();
            from.Friendships.ForEach(to.Friendships.Add);

            //to.FriendRequests= from.FriendRequests;
            to.FriendRequests.Clear();
            from.FriendRequests.ForEach(to.FriendRequests.Add);
        }
    }
}