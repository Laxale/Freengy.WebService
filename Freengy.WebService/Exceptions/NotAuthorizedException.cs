// Created by Laxale 21.04.2018
//
//

using System;


namespace Freengy.WebService.Exceptions 
{
    /// <summary>
    /// Exception about not authorized request failure.
    /// </summary>
    internal class NotAuthorizedException : Exception 
    {
        public NotAuthorizedException(Guid senderId) 
        {
            SenderId = senderId;
        }


        /// <summary>
        /// Request sender identifier.
        /// </summary>
        public Guid SenderId { get; }
    }
}
