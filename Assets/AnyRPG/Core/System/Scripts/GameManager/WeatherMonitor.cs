using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class WeatherMonitor : ConfiguredClass {

        // state tracking
        private WeatherProfile previousWeather = null;
        private WeatherProfile currentWeather = null;

        private int sceneHandle = 0;
        private SceneNode sceneNode = null;
        private List<WeatherWeightNode> weatherWeights = new List<WeatherWeightNode>();

        private Coroutine weatherCoroutine = null;

        // game manager references
        private TimeOfDayManagerServer timeOfDayManagerServer = null;
        private WeatherManagerServer weatherManagerServer = null;

        public WeatherProfile CurrentWeather { get => currentWeather; }

        public WeatherMonitor(SystemGameManager systemGameManager, int sceneHandle, SceneNode sceneNode) {
            //Debug.Log($"WeatherMonitor.WeatherMonitor({sceneHandle}, {sceneNode.ResourceName})");

            this.sceneNode = sceneNode;
            this.sceneHandle = sceneHandle;
            Configure(systemGameManager);
            SetupWeatherList();
            ChooseWeather();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            timeOfDayManagerServer = systemGameManager.TimeOfDayManagerServer;
            weatherManagerServer = systemGameManager.WeatherManagerServer;
        }

        private void SetupWeatherList() {
            //Debug.Log($"WeatherMonitor.SetupWeatherList()");

            WeatherWeightNode clearWeatherWeightNode = new WeatherWeightNode();
            clearWeatherWeightNode.Weight = sceneNode.NoWeatherWeight;
            weatherWeights.Add(clearWeatherWeightNode);
            weatherWeights.AddRange(sceneNode.WeatherWeights);
        }

        private void ChooseWeather() {
            //Debug.Log("WeatherMonitor.ChooseWeather()");

            if (weatherWeights.Count == 1) {
                // no weather to choose from (clear weather is always in the list)
                return;
            }

            int sumOfWeight = 0;
            int accumulatedWeight = 0;
            int usedIndex = 0;

            // get sum of all weights
            for (int i = 0; i < weatherWeights.Count; i++) {
                sumOfWeight += weatherWeights[i].Weight;
            }
            if (sumOfWeight == 0) {
                // there was weather, but it didn't have any weights
                return;
            }

            // perform weighted random roll to determine weather
            previousWeather = currentWeather;
            int rnd = UnityEngine.Random.Range(0, sumOfWeight);
            for (int i = 0; i < weatherWeights.Count; i++) {
                accumulatedWeight += (int)weatherWeights[i].Weight;
                if (rnd < accumulatedWeight) {
                    usedIndex = i;
                    break;
                }
            }
            currentWeather = weatherWeights[usedIndex].Weather;
            weatherManagerServer.ProcessChooseWeather(sceneHandle, currentWeather);

            if (currentWeather == previousWeather) {

                // weather is the same, just keep monitoring it
                EndWeatherMonitoring();
                StartWeatherMonitoring();
                return;
            }

            EndWeather(previousWeather);
            StartWeather();
        }

        private void StartWeatherMonitoring() {
            //Debug.Log($"WeatherMonitor.StartWeatherMonitoring()");

            weatherCoroutine = systemGameManager.StartCoroutine(MonitorWeather(sceneNode.RandomWeatherLength));
        }

        private void EndWeatherMonitoring() {
            //Debug.Log($"WeatherMonitor.EndWeatherMonitoring()");

            if (weatherCoroutine != null) {
                systemGameManager.StopCoroutine(weatherCoroutine);
                weatherCoroutine = null;
            }
        }

        private IEnumerator MonitorWeather(float inGameSeconds) {
            //Debug.Log($"WeatherMonitor.MonitorWeather({inGameSeconds})");

            DateTime startTime = timeOfDayManagerServer.InGameTime;
            DateTime endTime = startTime.AddSeconds(inGameSeconds);

            //Debug.Log($"WeatherMonitor.MonitorWeather({inGameSeconds}) start: {startTime.ToShortDateString()} {startTime.ToShortTimeString()} end: {endTime.ToLongDateString()} {endTime.ToShortTimeString()}");

            while (timeOfDayManagerServer.InGameTime < endTime) {
                yield return null;
            }
            ChooseWeather();
        }

        private void StartWeather() {
            //Debug.Log($"WeatherMonitor.StartWeather()");

            // always monitor weather, it could be clear
            StartWeatherMonitoring();
            weatherManagerServer.ProcessStartWeather(sceneHandle);
        }

        public void EndWeather() {
            //Debug.Log($"WeatherMonitor.EndWeather()");

            EndWeather(currentWeather, true);
        }

        private void EndWeather(WeatherProfile previousWeather) {
            //Debug.Log($"WeatherMonitor.EndWeather({(previousWeather == null ? "null" : previousWeather.ResourceName)})");

            EndWeather(previousWeather, false);
        }

        private void EndWeather(WeatherProfile previousWeather, bool immediate) {
            //Debug.Log($"WeatherMonitor.EndWeather({(previousWeather == null ? "null" : previousWeather.ResourceName)}, {immediate})");

            EndWeatherMonitoring();
            weatherManagerServer.ProcessEndWeather(sceneHandle, previousWeather, immediate);

            if (previousWeather == null) {
                // no previous weather, nothing to do
                return;
            }

        }


    }

}

