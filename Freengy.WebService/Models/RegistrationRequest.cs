// Created by Laxale 17.04.2018
//
//

using System;

using Freengy.WebService.Enums;


namespace Freengy.WebService.Models 
{
    public class RegistrationRequest 
    {
        public string UserName { set; get; }

        public DateTime RequestTime { get; set; }

        public DateTime? RegistrationTime { get; set; }

        /// <summary>
        /// Registration status is set by server when processed request.
        /// </summary>
        public RegistrationStatus Status { get; set; }
    }
}