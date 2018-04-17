// Created by Laxale 17.04.2018
//
//

using System.Threading.Tasks;

using Freengy.WebService.Enums;


namespace Freengy.WebService.Models 
{
    internal class LogInRequest 
    {
        /// <summary>
        /// Name of user trying to log in.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Hash of user password.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Status is set by server when processed request.
        /// </summary>
        public AccountOnlineStatus LogInStatus { get; set; }
    }
}