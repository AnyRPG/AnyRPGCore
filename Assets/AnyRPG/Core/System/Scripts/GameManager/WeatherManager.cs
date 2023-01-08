using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class WeatherManager : ConfiguredMonoBehaviour {

        // state tracking
        private WeatherProfile previousWeather = null;
        private WeatherProfile currentWeather = null;
        private WeatherEffectController weatherEffectController = null;
        private List<WeatherWeightNode> weatherWeights = new List<WeatherWeightNode>();
        private Coroutine weatherCoroutine = null;
        private Coroutine fogCoroutine = null;
        private Coroutine shadowCoroutine = null;
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
        protected SystemDataFactory systemDataFactory = null;
        protected LevelManager levelManager = null;
        protected AudioManager audioManager = null;
        protected ObjectPooler objectPooler = null;
        protected PlayerManager playerManager = null;
        protected CameraManager cameraManager = null;
        protected TimeOfDayManager timeOfDayManager = null;

        public AudioClip CurrentAmbientSound { get => currentAmbientSound; set => currentAmbientSound = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
            levelManager = systemGameManager.LevelManager;
            audioManager = systemGameManager.AudioManager;
            objectPooler = systemGameManager.ObjectPooler;
            playerManager = systemGameManager.PlayerManager;
            cameraManager = systemGameManager.CameraManager;
            timeOfDayManager = systemGameManager.TimeOfDayManager;
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("WeatherManager.HandleLevelUnload()");

            EndWeather(currentWeather, true);
            CleanupWeatherEffectControllers();
            previousWeather = null;
            currentWeather = null;
            defaultShadowStrength = 1f;
            sunLight = null;
            fogList.Clear();
        }

        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("WeatherManager.HandleLevelLoad()");

            GetSceneFogSettings();
            GetSceneShadowSettings();
            if (levelManager.GetActiveSceneNode() != null) {
                SetupWeatherList();
                ChooseWeather();
            }
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            // the weather may have spawned before the player.  If so, the weather needs to be set to follow the player now that it is spawned

            if (currentWeather == null) {
                // no weather, nothing to do
                return;
            }

            FollowPlayer();
        }

        private void GetSceneShadowSettings() {
            if (RenderSettings.sun != null) {
                sunLight = RenderSettings.sun;
                defaultShadowStrength = sunLight.shadowStrength;
            }
        }

        private void GetSceneFogSettings() {
            defaultFogSettings.UseFog = RenderSettings.fog;
            defaultFogSettings.FogColor = RenderSettings.fogColor;
            defaultFogSettings.FogDensity = RenderSettings.fogDensity;
            fogList.Add(defaultFogSettings);
        }

        public void ActivateWeatherFogSettings(FogSettings fogSettings) {
            weatherFogSettings = fogSettings;
            fogList.Add(weatherFogSettings);
            if (currentFogSettings != waterFogSettings) {
                currentFogSettings = weatherFogSettings;
                //ActivateCurrentFogSettings();
                FadeToFogSettings();
            }
        }

        public void DeactivateWeatherFogSettings() {
            fogList.Remove(weatherFogSettings);
            if (currentFogSettings != waterFogSettings) {
                currentFogSettings = defaultFogSettings;
                //ActivateCurrentFogSettings();
                FadeToFogSettings();
            }
        }

        private void ActivateCurrentFogSettings() {
            if (fogCoroutine != null) {
                StopCoroutine(fogCoroutine);
            }
            RenderSettings.fog = currentFogSettings.UseFog;
            RenderSettings.fogColor = currentFogSettings.FogColor;
            RenderSettings.fogDensity = currentFogSettings.FogDensity;
        }

        private void FadeToFogSettings() {
            if (fogCoroutine != null) {
                StopCoroutine(fogCoroutine);
            }
            fogCoroutine = StartCoroutine(FogFade(3f));
        }

        private void FadeToShadowSettings() {

            if (sunLight == null) {
                // there is no sun so sun shadows cannot be faded
                return;
            }

            if (shadowCoroutine != null) {
                StopCoroutine(shadowCoroutine);
            }
            shadowCoroutine = StartCoroutine(ShadowFade(3f));
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

        private void SetupWeatherList() {
            weatherWeights.Clear();

            WeatherWeightNode clearWeatherWeightNode = new WeatherWeightNode();
            clearWeatherWeightNode.Weight = levelManager.GetActiveSceneNode().NoWeatherWeight;
            weatherWeights.Add(clearWeatherWeightNode);
            weatherWeights.AddRange(levelManager.GetActiveSceneNode().WeatherWeights);
        }

        private void ChooseWeather() {
            //Debug.Log("WeatherManager.ChooseWeather()");

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

            // always get ambient sound even if current weather is null
            // because the sound needs to be set to null or it will keep playing
            // the old weather sound when switched to clear weather
            // this is done here rather than in startWeather to avoid a crossfade to the wrong sound between EndWeather() and StartWeather()
            //currentAmbientSound = currentWeather?.AmbientSound;

            //Debug.Log("WeatherManager.ChooseWeather() picked: " + (currentWeather == null ? "clear" : currentWeather.DisplayName));

            if (currentWeather == previousWeather) {

                // weather is the same, just keep monitoring it
                EndWeatherMonitoring();
                StartWeatherMonitoring();
                return;
            }

            EndWeather(previousWeather);
            StartWeather();
        }

        private void StartWeather() {
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
                    timeOfDayManager.SuppressAmbientSounds();
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
            timeOfDayManager.PlayAmbientSounds(3);

            // always monitor weather, it could be clear
            StartWeatherMonitoring();
        }

        private void EndWeather(WeatherProfile previousWeather, bool immediate) {
            
            currentAmbientSound = null;

            if (immediate == true) {
                // this is only done on level unload because shadow fading is normally done in StartWeather()
                if (shadowCoroutine != null) {
                    StopCoroutine(shadowCoroutine);
                    shadowCoroutine = null;
                }
            }

            EndWeatherMonitoring();

            if (previousWeather == null) {
                // no previous weather, nothing to do
                return;
            }

            DeactivateWeatherFogSettings();

            if (previousWeather.SuppressAmbientSounds == true) {
                timeOfDayManager.AllowAmbientSounds();
            }

            if (weatherEffectController != null) {
                if (immediate == true) {
                    weatherEffectController.StopPlaying(immediate);
                    objectPooler.ReturnObjectToPool(weatherEffectController.gameObject);
                } else {
                    StartCoroutine(WeatherFade(weatherEffectController, weatherEffectController.FadeTime));
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
            fadingControllers.Remove(weatherEffectController);
            objectPooler.ReturnObjectToPool(weatherEffectController.gameObject);
        }

        private void CleanupWeatherEffectControllers() {
            StopAllCoroutines();
            foreach (WeatherEffectController weatherEffectController in fadingControllers) {
                weatherEffectController.StopPlaying(true);
                objectPooler.ReturnObjectToPool(weatherEffectController.gameObject);
            }
            fadingControllers.Clear();
        }

        private void StartWeatherMonitoring() {
            weatherCoroutine = StartCoroutine(MonitorWeather(levelManager.GetActiveSceneNode().RandomWeatherLength));
        }

        private void EndWeatherMonitoring() {
            if (weatherCoroutine != null) {
                StopCoroutine(weatherCoroutine);
                weatherCoroutine = null;
            }
        }

        private IEnumerator MonitorWeather(float inGameSeconds) {
            //Debug.Log($"WeatherManager.MonitorWeather({inGameSeconds})");

            DateTime startTime = timeOfDayManager.InGameTime;
            DateTime endTime = startTime.AddSeconds(inGameSeconds);

            //Debug.Log($"WeatherManager.MonitorWeather({inGameSeconds}) start: {startTime.ToShortDateString()} {startTime.ToShortTimeString()} end: {endTime.ToLongDateString()} {endTime.ToShortTimeString()}");

            while (timeOfDayManager.InGameTime < endTime) {
                yield return null;
            }
            ChooseWeather();
        }

        private void FollowPlayer() {
            if (weatherEffectController != null && playerManager.ActiveUnitController != null) {
                weatherEffectController.SetTarget(cameraManager.MainCameraGameObject);
            }
        }


    }

    [System.Serializable]
    public class FogSettings {

        [SerializeField]
        private bool useFog = false;

        [SerializeField]
        private Color fogColor = new Color32(128, 128, 128, 255);

        [SerializeField]
        [Range(0, 1)]
        private float fogDensity = 0.05f;

        public FogSettings() {
        }

        public FogSettings(bool useFog, Color fogColor, float fogDensity) {
            this.useFog = useFog;
            this.fogColor = fogColor;
            this.fogDensity = fogDensity;
        }

        public bool UseFog { get => useFog; set => useFog = value; }
        public Color FogColor { get => fogColor; set => fogColor = value; }
        public float FogDensity { get => fogDensity; set => fogDensity = value; }
    }

}