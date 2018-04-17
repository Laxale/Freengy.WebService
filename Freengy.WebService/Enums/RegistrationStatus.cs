// Created by Laxale 17.04.2018
//
//


namespace Freengy.WebService.Enums 
{
    /// <summary>
    /// Represents possible results of a new user registration process.
    /// </summary>
    public enum RegistrationStatus 
    {
        /// <summary>
        /// Default value - undefined.
        /// </summary>
        None,

        /// <summary>
        /// User already exists.
        /// </summary>
        AlreadyExists,

        /// <summary>
        /// User registered.
        /// </summary>
        Registered,

        /// <summary>
        /// Error occured during registration.
        /// </summary>
        Error
    }
}