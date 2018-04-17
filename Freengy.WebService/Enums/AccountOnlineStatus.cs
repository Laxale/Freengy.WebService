// Created by Laxale 17.04.2018
//
//


namespace Freengy.WebService.Enums 
{
    internal enum AccountOnlineStatus 
    {
        /// <summary>
        /// Undefined value.
        /// </summary>
        None,

        /// <summary>
        /// Account is online.
        /// </summary>
        Online,

        /// <summary>
        /// Account is offline.
        /// </summary>
        Offline,

        /// <summary>
        /// Account is online and busy.
        /// </summary>
        Busy,

        /// <summary>
        /// Account is online and AFK.
        /// </summary>
        Afk,

        /// <summary>
        /// Account is doesnt want to be disturbed.
        /// </summary>
        DoNotDisturb,

        // statuses that are not for user interaction - just for service responding

        /// <summary>
        /// Some error occured when logging account in.
        /// </summary>
        Error,

        /// <summary>
        /// Account is already logged in.
        /// </summary>
        AlreadyLoggedIn,

        /// <summary>
        /// Account cant be logged in as it is not registered.
        /// </summary>
        DoesntExist
    }
}