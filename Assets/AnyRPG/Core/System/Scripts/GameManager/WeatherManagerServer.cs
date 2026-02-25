using FishNet.Managing.Scened;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class WeatherManagerServer : ConfiguredClass {


        private Dictionary<int, WeatherMonitor> weatherMonitors = new Dictionary<int, WeatherMonitor>();

        // game manager references
        protected LevelManagerClient levelManagerClient = null;
        protected PlayerManagerClient playerManagerClient = null;
        protected CameraManager cameraManager = null;
        protected TimeOfDayManagerServer timeOfDayManagerServer = null;
        protected TimeOfDayManagerClient timeOfDayManagerClient = null;
        protected SystemEventManager systemEventManager = null;
        protected LevelManagerServer levelManagerServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            levelManagerServer.OnAddLoadedScene += HandleAddLoadedScene;
            levelManagerServer.OnRemoveLoadedScene += HandleRemoveLoadedScene;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            levelManagerClient = systemGameManager.LevelManagerClient;
            playerManagerClient = systemGameManager.PlayerManagerClient;
            cameraManager = systemGameManager.CameraManager;
            timeOfDayManagerServer = systemGameManager.TimeOfDayManagerServer;
            timeOfDayManagerClient = systemGameManager.TimeOfDayManagerClient;
            systemEventManager = systemGameManager.SystemEventManager;
            levelManagerServer = systemGameManager.LevelManagerServer;
        }

        private void HandleRemoveLoadedScene(int sceneHandle, string sceneName) {
            //Debug.Log($"WeatherManagerServer.HandleRemoveLoadedScene({sceneHandle}, {sceneName})");

            ProcessRemoveLoadedScene(sceneHandle, sceneName);
        }

        private void HandleAddLoadedScene(int sceneHandle, SceneData sceneData) {
            //Debug.Log($"WeatherManagerServer.HandleAddLoadedScene({sceneHandle}, {sceneName})");

            if (sceneData.SceneNode != null) {
                ProcessAddLoadedScene(sceneHandle, sceneData.SceneNode);
            }
        }

        private void ProcessAddLoadedScene(int sceneHandle, SceneNode sceneNode) {
            //Debug.Log($"WeatherManagerServer.ProcessAddLoadedScene({sceneHandle}, {sceneNode.ResourceName})");

            if (levelManagerClient.IsMainMenu(sceneNode.SceneName) == true || levelManagerClient.IsInitializationScene(sceneNode.SceneName) == true) {
                return;
            }
            weatherMonitors.Add(sceneHandle, new WeatherMonitor(systemGameManager, sceneHandle, sceneNode));
        }

        private void ProcessRemoveLoadedScene(int sceneHandle, string sceneName) {
            //Debug.Log($"WeatherManagerServer.ProcessRemoveLoadedScene({sceneHandle}, {sceneName})");

            if (weatherMonitors.ContainsKey(sceneHandle) == true) {
                weatherMonitors[sceneHandle].EndWeather();
                weatherMonitors.Remove(sceneHandle);
            }
        }

        public void ProcessEndWeather(int sceneHandle, WeatherProfile previousWeather, bool immediate) {
            //Debug.Log($"WeatherManagerServer.ProcessEndWeather({sceneHandle}, {(previousWeather == null ? "null" : previousWeather.ResourceName)}, {immediate})");

            systemEventManager.NotifyOnEndWeather(sceneHandle, previousWeather, immediate);
        }

        public void ProcessChooseWeather(int sceneHandle, WeatherProfile currentWeather) {
            //Debug.Log($"WeatherManagerServer.ChooseWeather({sceneHandle}, {(currentWeather == null ? "null" : currentWeather?.ResourceName)})");

            systemEventManager.NotifyOnChooseWeather(sceneHandle, currentWeather);
        }

        public void ProcessStartWeather(int sceneHandle) {
            //Debug.Log($"WeatherManagerServer.StartWeather({sceneHandle})");

            systemEventManager.NotifyOnStartWeather(sceneHandle);
        }

        public WeatherProfile GetSceneWeatherProfile(int handle) {
            if (weatherMonitors.ContainsKey(handle) == true) {
                return weatherMonitors[handle].CurrentWeather;
            } else {
                //Debug.LogWarning($"WeatherManagerServer.GetSceneWeatherProfile({handle}) - no weather monitor found for this scene");
                return null;
            }
        }
    }

}