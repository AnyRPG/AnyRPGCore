using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class WeatherManagerClient : ConfiguredClass {

        // state tracking
        private WeatherProfile previousWeather = null;
        private WeatherProfile currentWeather = null;
        private WeatherEffectController weatherEffectController = null;
        private Coroutine fogCoroutine = null;
        private Coroutine shadowCoroutine = null;
        private Coroutine weatherCoroutine = null;
        private AudioClip currentAmbientSound = null;
        private FogSettings defaultFogSettings = new FogSettings();
        private FogSettings weatherFogSettings = new FogSettings();
        private FogSettings waterFogSettings = new FogSettings();
        private FogSettings currentFogSettings = null;
        private List<WeatherEffectController> fadingControllers = new List<WeatherEffectController>();
        // keep a list of fog settings for overrides
        private List<FogSettings> fogList = new List<FogSettings>();
        private float defaultShadowStrength = 1f;
        private Light sunLight = null;

        // game manager references
        protected LevelManager levelManager = null;
        protected AudioManager audioManager = null;
        protected ObjectPooler objectPooler = null;
        protected PlayerManager playerManager = null;
        protected CameraManager cameraManager = null;
        protected TimeOfDayManagerServer timeOfDayManagerServer = null;
        protected TimeOfDayManagerClient timeOfDayManagerClient = null;
        protected SystemEventManager systemEventManager = null;

        public AudioClip CurrentAmbientSound { get => currentAmbientSound; set => currentAmbientSound = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemEventManager.OnLevelUnloadClient += HandleLevelUnload;
            systemEventManager.OnLevelLoad += HandleLevelLoad;
            systemEventManager.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            systemEventManager.OnChooseWeather += HandleChooseWeather;
            systemEventManager.OnStartWeather += HandleStartWeather;
            systemEventManager.OnEndWeather += HandleEndWeather;
            networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandlStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            levelManager = systemGameManager.LevelManager;
            audioManager = systemGameManager.AudioManager;
            objectPooler = systemGameManager.ObjectPooler;
            playerManager = systemGameManager.PlayerManager;
            cameraManager = systemGameManager.CameraManager;
            timeOfDayManagerServer = systemGameManager.TimeOfDayManagerServer;
            timeOfDayManagerClient = systemGameManager.TimeOfDayManagerClient;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        private void HandleStartServer() {
            // weather manager client should not be listening for weather events when the server is running
            systemEventManager.OnChooseWeather -= HandleChooseWeather;
            systemEventManager.OnStartWeather -= HandleStartWeather;
            systemEventManager.OnEndWeather -= HandleEndWeather;
        }

        private void HandlStopServer() {
            systemEventManager.OnChooseWeather += HandleChooseWeather;
            systemEventManager.OnStartWeather += HandleStartWeather;
            systemEventManager.OnEndWeather += HandleEndWeather;
        }

        private void HandleStartWeather(int sceneHandle) {
            StartWeather();
        }

        private void HandleEndWeather(int sceneHandle, WeatherProfile profile, bool immediate) {
            EndWeather(profile, immediate);
        }

        private void HandleChooseWeather(int sceneHandle, WeatherProfile weatherProfile) {
            //Debug.Log($"WeatherManagerClient.HandleChooseWeather({sceneHandle}, {(weatherProfile == null ? "null" : weatherProfile.ResourceName)})");

            ChooseWeather(weatherProfile);
        }


        public void HandleLevelUnload(int sceneHandle, string sceneName) {
            //Debug.Log($"WeatherManagerClient.HandleLevelUnload({sceneHandle}, {sceneName})");

            if (levelManager.IsMainMenu(sceneName) == true || levelManager.IsInitializationScene(sceneName) == true) {
                // there is no weather in the main menu or initialization scene, so nothing to do
                return;
            }
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) {
                EndWeather(currentWeather, true);
            }
            CleanupWeatherEffectControllers();
            previousWeather = null;
            currentWeather = null;
            defaultShadowStrength = 1f;
            sunLight = null;
            fogList.Clear();
        }

        public void HandleLevelLoad() {
            //Debug.Log("WeatherManagerClient.HandleLevelLoad()");

            if (networkManagerServer.ServerModeActive == true) {
                return;
            }
            if (levelManager.IsMainMenu() == true || levelManager.IsInitializationScene() == true) {
                return;
            }
            GetSceneFogSettings();
            GetSceneShadowSettings();
            if (systemGameManager.GameMode == GameMode.Local || levelManager.IsCutscene()) {
                return;
            }
            // if the game is not in local mode, then the weather needs to be requested from the server
            networkManagerClient.RequestSceneWeather();
        }

        public void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log("WeatherManagerClient.HandlePlayerUnitSpawn()");
            // the weather may have spawned before the player.  If so, the weather needs to be set to follow the player now that it is spawned

            if (currentWeather == null) {
                // no weather, nothing to do
                return;
            }

            FollowPlayer();
        }

        private void GetSceneShadowSettings() {
            //Debug.Log("WeatherManagerClient.GetSceneShadowSettings()");
            if (RenderSettings.sun != null) {
                sunLight = RenderSettings.sun;
                defaultShadowStrength = sunLight.shadowStrength;
            }
        }

        private void GetSceneFogSettings() {
            //Debug.Log("WeatherManagerClient.GetSceneFogSettings()");

            defaultFogSettings.UseFog = RenderSettings.fog;
            defaultFogSettings.FogColor = RenderSettings.fogColor;
            defaultFogSettings.FogDensity = RenderSettings.fogDensity;
            fogList.Add(defaultFogSettings);
        }

        public void ActivateWeatherFogSettings(FogSettings fogSettings) {
            //Debug.Log($"WeatherManagerClient.ActivateWeatherFogSettings()");

            weatherFogSettings = fogSettings;
            fogList.Add(weatherFogSettings);
            if (currentFogSettings != waterFogSettings) {
                currentFogSettings = weatherFogSettings;
                //ActivateCurrentFogSettings();
                FadeToFogSettings();
            }
        }

        public void DeactivateWeatherFogSettings() {
            //Debug.Log($"WeatherManagerClient.DeactivateWeatherFogSettings()");

            fogList.Remove(weatherFogSettings);
            if (currentFogSettings != waterFogSettings) {
                currentFogSettings = defaultFogSettings;
                //ActivateCurrentFogSettings();
                FadeToFogSettings();
            }
        }

        private void ActivateCurrentFogSettings() {
            //Debug.Log($"WeatherManagerClient.ActivateCurrentFogSettings()");

            if (fogCoroutine != null) {
                systemGameManager.StopCoroutine(fogCoroutine);
            }
            RenderSettings.fog = currentFogSettings.UseFog;
            RenderSettings.fogColor = currentFogSettings.FogColor;
            RenderSettings.fogDensity = currentFogSettings.FogDensity;
        }

        private void FadeToFogSettings() {
            if (fogCoroutine != null) {
                systemGameManager.StopCoroutine(fogCoroutine);
            }
            fogCoroutine = systemGameManager.StartCoroutine(FogFade(3f));
        }

        private void FadeToShadowSettings() {

            if (sunLight == null) {
                // there is no sun so sun shadows cannot be faded
                return;
            }

            if (shadowCoroutine != null) {
                systemGameManager.StopCoroutine(shadowCoroutine);
            }
            shadowCoroutine = systemGameManager.StartCoroutine(ShadowFade(3f));
        }

        private IEnumerator ShadowFade(float fadeTime) {

            float elapsedTime = 0f;
            float originalShadowStrength = sunLight.shadowStrength;
            float targetShadowStrength = 1f;
            if (currentWeather == null) {
                targetShadowStrength = defaultShadowStrength;
            } else {
                targetShadowStrength = defaultShadowStrength * currentWeather.ShadowStrength;
            }

            float shadowDelta = targetShadowStrength - originalShadowStrength;

            while (elapsedTime <= fadeTime) {
                sunLight.shadowStrength = originalShadowStrength + ((elapsedTime / fadeTime) * shadowDelta);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            shadowCoroutine = null;
        }

        private IEnumerator FogFade(float fadeTime) {
            
            float elapsedTime = 0f;
            Color originalFogColor = RenderSettings.fogColor;

            // if fog will be turned off, target fog density is considered to be zero rather than whatever hidden value it actually has
            // if fog will be turned on, we get the actual target density below
            float targetFogDensity = 0f;
            if (currentFogSettings.UseFog == true) {
                targetFogDensity = currentFogSettings.FogDensity;
            }

            // if fog was turned off, current fog density is considered to be zero rather than whatever hidden value it actually had
            // if fog was turned on, we get the actual density below
            float currentFogDensity = 0f;
            if (RenderSettings.fog == true) {
                currentFogDensity = RenderSettings.fogDensity;
            }

            float fogDelta = targetFogDensity - currentFogDensity;

            // whether fading in or out, fog needs to be on so it can slowly fade rather then just turning on/off at the end of the coroutine
            RenderSettings.fog = true;

            while (elapsedTime <= fadeTime) {
                RenderSettings.fogDensity = currentFogDensity + ((elapsedTime / fadeTime) * fogDelta);

                RenderSettings.fogColor = new Color32(
                    (byte)(255f * (originalFogColor.r + ((elapsedTime / fadeTime) * (currentFogSettings.FogColor.r - originalFogColor.r)))),
                    (byte)(255f * (originalFogColor.g + ((elapsedTime / fadeTime) * (currentFogSettings.FogColor.g - originalFogColor.g)))),
                    (byte)(255f * (originalFogColor.b + ((elapsedTime / fadeTime) * (currentFogSettings.FogColor.b - originalFogColor.b)))),
                    (byte)(255f * (originalFogColor.a + ((elapsedTime / fadeTime) * (currentFogSettings.FogColor.a - originalFogColor.a))))
                    );
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            ActivateCurrentFogSettings();
            fogCoroutine = null;
        }

        public void ActivateWaterFogSettings(bool useFog, Color fogColor, float fogDensity) {
            waterFogSettings.UseFog = useFog;
            waterFogSettings.FogColor = fogColor;
            waterFogSettings.FogDensity = fogDensity;
            currentFogSettings = waterFogSettings;
            ActivateCurrentFogSettings();
        }

        public void DeactivateWaterFogSettings() {
            currentFogSettings = fogList[fogList.Count - 1];
            ActivateCurrentFogSettings();
        }

        public void ChooseWeather(WeatherProfile weatherProfile) {
            //Debug.Log($"WeatherManagerClient.ChooseWeather({(weatherProfile == null ? "null" : weatherProfile.ResourceName)})");

            previousWeather = currentWeather;
            currentWeather = weatherProfile;
        }

        public void StartWeather() {
            //Debug.Log($"WeatherManagerClient.StartWeather()");

            if (currentWeather != null) {
                if (currentWeather.PrefabProfile?.Prefab != null) {
                    GameObject prefabObject = objectPooler.GetPooledObject(currentWeather.PrefabProfile.Prefab);
                    weatherEffectController = prefabObject.GetComponent<WeatherEffectController>();
                    if (weatherEffectController != null) {
                        weatherEffectController.StartPlaying();
                    }
                    FollowPlayer();
                }

                if (currentWeather.SuppressAmbientSounds == true) {
                    timeOfDayManagerClient.SuppressAmbientSounds();
                }

                ActivateWeatherFogSettings(currentWeather.FogSettings);
            }

            // always perform shadow fade because the weather could have changed from something to clear
            FadeToShadowSettings();

            // always get ambient sound even if current weather is null
            // because the sound needs to be set to null or it will keep playing
            // the old weather sound when switched to clear weather
            // MOVED TO CHOOSEWEATHER
            currentAmbientSound = currentWeather?.AmbientSound;

            // always play ambient sounds
            // it could be clear weather so the default ambient sounds for the scene should be played
            timeOfDayManagerClient.PlayAmbientSounds(3);

        }

        public void EndWeather(WeatherProfile previousWeather, bool immediate) {
            //Debug.Log($"WeatherManagerClient.EndWeather({(previousWeather == null ? "null" : previousWeather.ResourceName)}, {immediate})");

            currentAmbientSound = null;

            if (immediate == true) {
                // this is only done on level unload because shadow fading is normally done in StartWeather()
                if (shadowCoroutine != null) {
                    systemGameManager.StopCoroutine(shadowCoroutine);
                    shadowCoroutine = null;
                }
            }

            if (previousWeather == null) {
                // no previous weather, nothing to do
                return;
            }

            DeactivateWeatherFogSettings();

            if (previousWeather.SuppressAmbientSounds == true) {
                timeOfDayManagerClient.AllowAmbientSounds();
            }

            if (weatherEffectController != null) {
                if (immediate == true) {
                    weatherEffectController.StopPlaying(immediate);
                    objectPooler.ReturnObjectToPool(weatherEffectController.gameObject);
                } else {
                    weatherCoroutine = systemGameManager.StartCoroutine(WeatherFade(weatherEffectController, weatherEffectController.FadeTime));
                }
                weatherEffectController = null;
            }
        }

        private void EndWeather(WeatherProfile previousWeather) {

            EndWeather(previousWeather, false);
        }

        private IEnumerator WeatherFade(WeatherEffectController weatherEffectController, float fadeTime) {
            float elapsedTime = 0f;
            fadingControllers.Add(weatherEffectController);
            weatherEffectController.StopPlaying();

            while (elapsedTime < fadeTime) {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            weatherCoroutine = null;
            fadingControllers.Remove(weatherEffectController);
            objectPooler.ReturnObjectToPool(weatherEffectController.gameObject);
        }

        private void CleanupWeatherEffectControllers() {
            if (fogCoroutine != null) {
                systemGameManager.StopCoroutine(fogCoroutine);
            }
            if (shadowCoroutine != null) {
                systemGameManager.StopCoroutine(shadowCoroutine);
            }
            if (weatherCoroutine != null) {
                systemGameManager.StopCoroutine(weatherCoroutine);
            }
            foreach (WeatherEffectController weatherEffectController in fadingControllers) {
                weatherEffectController.StopPlaying(true);
                objectPooler.ReturnObjectToPool(weatherEffectController.gameObject);
            }
            fadingControllers.Clear();
        }

        private void FollowPlayer() {
            if (weatherEffectController != null && playerManager.ActiveUnitController != null) {
                weatherEffectController.SetTarget(cameraManager.MainCameraGameObject);
            }
        }


    }

}