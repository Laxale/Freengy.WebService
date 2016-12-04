// Created by Laxale 04.12.2016
//
//


namespace Freengy.SharedWebTypes.Objects 
{
    using System;

    public sealed class RegistrationRequest 
    {
        public RegistrationRequest(string userName) 
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));

            this.UserName = userName;
        }

        public string UserName { get; }
    }
}