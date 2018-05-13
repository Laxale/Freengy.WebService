// Created by Laxale 13.05.2018
//
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Freengy.Common.Helpers;
using Freengy.WebService.Interfaces;
using Freengy.WebService.Models;


namespace Freengy.WebService.Services 
{
    /// <summary>
    /// Управляет системой начисления опыта аккаунтам.
    /// </summary>
    internal class ExpirienceService : IService 
    {
        private static readonly object Locker = new object();

        private static ExpirienceService instance;

        private readonly int expAdditionTimeout = 300000;
        private readonly DelayedEventInvoker delayedInvoker;
        private readonly AccountStateService stateService = AccountStateService.Instance;
        private readonly UserInformerService informerService = UserInformerService.Instance;


        private ExpirienceService() 
        {
            delayedInvoker = new DelayedEventInvoker(expAdditionTimeout);
        }


        /// <summary>
        /// Единственный инстанс <see cref="ExpirienceService"/>.
        /// </summary>
        public static ExpirienceService Instance 
        {
            get
            {
                lock (Locker)
                {
                    return instance ?? (instance = new ExpirienceService());
                }
            }
        }


        /// <summary>
        /// Initialize the service.
        /// </summary>
        public void Initialize() 
        {
            delayedInvoker.DelayedEvent += OnDelayedEvent;
            delayedInvoker.RequestDelayedEvent();
        }

        private void OnDelayedEvent() 
        {
            IEnumerable<ComplexAccountState> onlineStates = stateService.GetAllOnline();

            Parallel.ForEach(onlineStates, state =>
            {
                informerService.NotifyUserOfExpAddition();
            });

            delayedInvoker.RequestDelayedEvent();
        }
    }
}