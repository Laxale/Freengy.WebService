// Created by Laxale 21.04.2018
//
//

using System;


namespace Freengy.WebService.Exceptions
{
    /// <summary>
    /// Exception about not found user name.
    /// </summary>
    internal class UserNotFoundException : Exception 
    {
        public UserNotFoundException(string userName) 
        {
            UserName = userName;
        }


        /// <summary>
        /// User name that was not found.
        /// </summary>
        public string UserName { get; }
    }
}