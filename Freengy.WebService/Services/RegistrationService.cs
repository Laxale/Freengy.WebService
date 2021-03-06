﻿// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Models;
using Freengy.Common.Restrictions;
using Freengy.Database.Context;
using Freengy.WebService.Context;
using Freengy.WebService.Extensions;
using Freengy.WebService.Helpers;
using Freengy.WebService.Interfaces;
using Freengy.WebService.Models;

using NLog;


namespace Freengy.WebService.Services 
{
    /// <summary>
    /// Service to manage user account registrations.
    /// </summary>
    internal class RegistrationService : IService 
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly object Locker = new object();

        private static RegistrationService instance;

        private readonly PasswordService passwordService = PasswordService.Instance;
        private readonly AccountStateService stateService = AccountStateService.Instance;
        //private readonly List<ComplexUserAccount> registeredAccounts = new List<ComplexUserAccount>();


        private RegistrationService() { }


        /// <summary>
        /// Единственный инстанс <see cref="RegistrationService"/>.
        /// </summary>
        public static RegistrationService Instance 
        {
            get
            {
                lock (Locker)
                {
                    return instance ?? (instance = new RegistrationService());
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
                //using (var dbContext = new ComplexUserContext())
                //{
                  //  List<ComplexUserAccount> accounts = dbContext.Objects.ToList();

                    //registeredAccounts.AddRange(accounts);
                //}

                //$"Cached {registeredAccounts.Count} accounts".WriteToConsole();

                logger.Info($"Initialized {nameof(RegistrationService)}");
            }
            catch (Exception ex)
            {
                ex.Message.WriteToConsole(ConsoleColor.Red);
                logger.Error($"Failed to initialize {nameof(RegistrationService)}");
                throw;
            }
        }

        public RegistrationStatus RegisterAccount(RegistrationRequest request, out ComplexUserAccount registeredAcc) 
        {
            UserAccountModel existingAccount = FindByName(request.UserName);

            if (existingAccount != null)
            {
                registeredAcc = null;
                return RegistrationStatus.AlreadyExists;
            }

            var newAccount = new ComplexUserAccount
            {
                Name = request.UserName
            };

            Password passwordData = passwordService.CreatePasswordData(request.Password);
            passwordData.ParentId = newAccount.Id;

            newAccount.PasswordDatas.Add(passwordData);

            RegistrationStatus result = RegisterAccount(newAccount, out newAccount);

            registeredAcc = newAccount;
            registeredAcc.PasswordData = registeredAcc.PasswordDatas.First();

            return result;
        }

        public ComplexUserAccount FindById(Guid userId) 
        {
            var foundUser = stateService.GetStatusOf(userId)?.ComplexAccount;

            return foundUser;
        }

        public ComplexUserAccount FindByName(string userName) 
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));

            var foundUser = stateService.GetStatusOf(userName)?.ComplexAccount;

            return foundUser;
        }

        public IEnumerable<ComplexUserAccount> FindByNameFilter(string nameFilter) 
        {
            if (string.IsNullOrWhiteSpace(nameFilter)) return new List<ComplexUserAccount>();

            IEnumerable<ComplexAccountState> filteredStates = stateService.GetByNameFilter(nameFilter);
            IEnumerable<ComplexUserAccount> filteredAccounts = filteredStates.Select(state => state.ComplexAccount);

            return filteredAccounts;
        }


        private RegistrationStatus RegisterAccount(ComplexUserAccount newAccount, out ComplexUserAccount registeredAcc) 
        {
            lock (Locker)
            {
                registeredAcc = null;

                try
                {
                    UserAccountModel trimmedAccount = new AccountValidator(newAccount).Trim();
                    ComplexUserAccount trimmedComplexAcc = trimmedAccount.ToComplex();
                    trimmedComplexAcc.PasswordDatas.AddRange(newAccount.PasswordDatas);

                    trimmedComplexAcc.RegistrationTime = DateTime.Now;

                    AccountDbInteracter.Instance.AddOrUpdate(trimmedComplexAcc);

                    stateService.RegisterNewUserState(trimmedComplexAcc);
                    passwordService.SaveOrUpdatePassword(newAccount.PasswordDatas[0], false);

                    registeredAcc = trimmedComplexAcc;

                    return RegistrationStatus.Registered;

                }
                catch (Exception ex)
                {
                    string message = $"Failed to register account '{newAccount.Name}': {ex.Message}";
                    logger.Error(ex, "Failed to register account");
                    message.WriteToConsole(ConsoleColor.Red);

                    return RegistrationStatus.Error;
                }
            }
        }
    }
}