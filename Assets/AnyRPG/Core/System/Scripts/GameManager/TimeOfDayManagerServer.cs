using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class TimeOfDayManagerServer : ConfiguredClass {

        // track the passage of in-game time
        private DateTime startTime;
        private DateTime inGameTime;

        // game manager references
        protected WeatherManagerClient weatherManager = null;
        protected SystemEventManager systemEventManager = null;

        public DateTime InGameTime { get => inGameTime; }
        public DateTime StartTime { get => startTime; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("TimeOfDayManagerServer.Configure()");

            base.Configure(systemGameManager);

            InitializeStartTime();
            CalculateRelativeTime();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            weatherManager = systemGameManager.WeatherManagerClient;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void Tick() {
            CalculateRelativeTime();
        }

        private void InitializeStartTime() {
            startTime = DateTime.Now;
        }

        /// <summary>
        /// calculate in-game time relative to real world time
        /// </summary>
        private void CalculateRelativeTime() {
            /*
            realCurrentTime = DateTime.Now;
            inGameTime = DateTime.Now;
            inGameTime = realCurrentTime.AddSeconds((realCurrentTime.TimeOfDay.TotalSeconds * systemConfigurationManager.TimeOfDaySpeed) - realCurrentTime.TimeOfDay.TotalSeconds);
            */
            // new calculation to always have start time set to actual current time
            inGameTime = startTime.AddSeconds((DateTime.Now - startTime).TotalSeconds * systemConfigurationManager.TimeOfDaySpeed);
            //Debug.Log("Time is " + inGameTime.ToShortTimeString());
            systemEventManager.NotifyOnCalculateRelativeTime();
        }

        public void SetStartTime(DateTime startTime) {
            this.startTime = startTime;
        }

    }

}