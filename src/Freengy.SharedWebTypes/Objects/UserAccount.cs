// Created by Laxale 02.12.2016
//
//


namespace Freengy.SharedWebTypes.Objects 
{
    using System;

    using Freengy.SharedWebTypes.Interfaces;

    public class UserAccount : IUserAccount 
    {
        public long Id { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
    }
}