// Created by Laxale 17.04.2018
//
//

using System;
using Freengy.WebService.Enums;


namespace Freengy.WebService.Models 
{
    internal class AccountState 
    {
        public AccountState(UserAccount account) 
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
        }

        
        public UserAccount Account { get; }
        
        public DateTime LastLogInTime { get; set; }

        public AccountOnlineStatus OnlineStatus { get; set; }
    }
}