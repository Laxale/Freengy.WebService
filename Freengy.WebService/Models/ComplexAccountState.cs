// Created by Laxale 27.04.2018
//
//

using System;

using Freengy.Common.Models;


namespace Freengy.WebService.Models 
{
    /// <summary>
    /// Data model of a user account state with more service properties.
    /// </summary>
    internal class ComplexAccountState 
    {
        public ComplexAccountState(AccountStateModel stateModel) 
        {
            StateModel = stateModel ?? throw new ArgumentNullException(nameof(stateModel));
        }


        /// <summary>
        /// Gets or sets value - how much times client didnt properly respond to state request (and will be considered offline).
        /// </summary>
        public int FailResponceCount { get; set; }

        /// <summary>
        /// Gets client account state model.
        /// </summary>
        public AccountStateModel StateModel { get; }

        /// <summary>
        /// Gets client auth model.
        /// </summary>
        public SessionAuth ClientAuth { get; } = new SessionAuth();
    }
}