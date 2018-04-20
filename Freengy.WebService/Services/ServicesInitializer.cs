// Created by Laxale 20.04.2018
//
//

using System;
using System.Collections.Generic;

using Freengy.WebService.Interfaces;


namespace Freengy.WebService.Services 
{
    /// <summary>
    /// Simple startup initializer of a known <see cref="IService"/> instances.
    /// </summary>
    internal class ServicesInitializer 
    {
        private readonly List<IService> services = new List<IService>();

        private bool inited;


        /// <summary>
        /// Initialize all registered <see cref="IService"/> instances (only once).
        /// </summary>
        public void InitRegistered() 
        {
            if (inited) return;

            foreach (IService service in services)
            {
                service.Initialize();
            }

            inited = true;
        }

        /// <summary>
        /// Registers a given <see cref="IService"/> to initialize it.
        /// </summary>
        /// <param name="service">Service to initialize.</param>
        /// <returns>this.</returns>
        public ServicesInitializer Register(IService service) 
        {
            if (!services.Contains(service))
            {
                services.Add(service);
            }

            return this;
        }
    }
}