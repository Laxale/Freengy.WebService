// Created by Laxale 03.12.2016
//
//


namespace Freengy.SharedWebTypes.Objects 
{
    /// <summary>
    /// Contains login information of request to the server
    /// </summary>
    public sealed class LoginModel 
    {
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        //public string ServerAddress { get; set; }
    }
}