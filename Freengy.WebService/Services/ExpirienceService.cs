// Created by Laxale 13.05.2018
//
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Freengy.Common.Enums;
using Freengy.Common.Helpers;
using Freengy.Common.Models;
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

        //private readonly int expAdditionTimeout = 6000; // 6 sec
        private readonly int expAdditionTimeout = 600000; // 10 min
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
                uint userPreviousLevel = state.ComplexAccount.Level;
                GainExpModel model = CalculateExpForAccount(state.ComplexAccount.Level);
                state.ComplexAccount.AddExp(model.Amount);
                informerService.NotifyUserOfExpAddition(state, model);
                
                uint userNewLevel = ExpirienceCalculator.GetLevelForExp((uint) state.ComplexAccount.Expirience);
                if (userNewLevel > userPreviousLevel)
                {
                    Task.Run(() => informerService.NotifyAllFriendsAboutUser(state));
                }
            });

            stateService.FlushStates();

            delayedInvoker.RequestDelayedEvent();
        }

        private GainExpModel CalculateExpForAccount(uint level) 
        {
            uint expAmount = ExpirienceCalculator.GetOnlineRewardForLevel(level);
            var expModel = new GainExpModel
            {
                Amount = expAmount,
                GainReason = GainExpReason.Online,
                TimeStamp = DateTime.Now
            };

            return expModel;
        }
    }
}