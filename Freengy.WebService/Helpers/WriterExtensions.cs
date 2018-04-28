// Created by Laxale 28.04.2018
//
//

using System;


namespace Freengy.WebService.Helpers 
{
    internal static class WriterExtensions 
    {
        /// <summary>
        /// Write a message to console with a given font color.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="inColor">Message text color.</param>
        public static void WriteToConsole(this string message, ConsoleColor inColor) 
        {
            Console.ForegroundColor = inColor;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}