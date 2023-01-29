using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class WeatherWeightNode : ConfiguredClass {

        [Tooltip("A Weather Profile that defines the weather settings.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(WeatherProfile))]
        private string weather = string.Empty;

        private WeatherProfile weatherReference = null;

        [Tooltip("A weighted value that determines the chance this weather is chosen.")]
        [SerializeField]
        private int weight = 0;

        public WeatherProfile Weather { get => weatherReference; set => weatherReference = value; }
        public int Weight { get => weight; set => weight = value; }

        public void SetupScriptableObjects(SystemGameManager systemGamenManager) {
            Configure(systemGamenManager);

            if (weather != null && weather != string.Empty) {
                WeatherProfile tmpWeather = systemDataFactory.GetResource<WeatherProfile>(weather);
                if (tmpWeather != null) {
                    weatherReference = tmpWeather;
                } else {
                    Debug.LogError("WeatherWeightNode.SetupScriptableObjects(): Could not find weather : " + weather + " while inititalizing weather.  CHECK INSPECTOR");
                }
            }
        }
    }

}