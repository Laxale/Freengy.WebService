// Created by Laxale 27.04.2018
//
//

using System;
using Freengy.Common.Enums;
using Freengy.Common.Models;

using Freengy.WebService.Extensions;


namespace Freengy.WebService.Models 
{
    /// <summary>
    /// Data model of a user account state with more service properties.
    /// </summary>
    internal class ComplexAccountState 
    {
        public ComplexAccountState(AccountStateModel stateModel) 
        {
            //StateModel = stateModel ?? throw new ArgumentNullException(nameof(stateModel));
            Address = stateModel?.Address ?? throw new ArgumentNullException(nameof(stateModel));
            OnlineStatus = stateModel.OnlineStatus;

            ComplexAccount = 
                stateModel.AccountModel is ComplexUserAccount complexAcc ? 
                    complexAcc :
                    stateModel.AccountModel.ToComplex();
        }


        /// <summary>
        /// Gets or sets value - how much times client didnt properly respond to state request (and will be considered offline).
        /// </summary>
        public int FailResponceCount { get; set; }

        /// <summary>
        /// Gets client account state model.
        /// </summary>
        //public AccountStateModel StateModel { get; }

        /// <summary>
        /// Возвращает ссылку на сложную модель аккаунта.
        /// </summary>
        public ComplexUserAccount ComplexAccount { get; }

        /// <summary>
        /// Текущий адрес пользователя.
        /// </summary>
        public string Address { get; set; }

        //public UserAccountModel AccountModel { get; set; }

        /// <summary>
        /// Текущий онлайн-статус пользователя.
        /// </summary>
        public AccountOnlineStatus OnlineStatus { get; set; }

        /// <summary>
        /// Gets client auth model.
        /// </summary>
        public SessionAuth ClientAuth { get; } = new SessionAuth();
    }
}