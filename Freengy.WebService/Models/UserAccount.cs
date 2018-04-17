// Created by Laxale 17.04.2018
//
//

using System;
using System.Threading.Tasks;

using Freengy.WebService.Interfaces;


namespace Freengy.WebService.Models
{
    internal class UserAccount : INamedObject, IObjectWithId 
    {
        public Guid Id { set; get; }

        public string Name { get; set; }

        public DateTime RegistrationTime { get; set; }
    }
}