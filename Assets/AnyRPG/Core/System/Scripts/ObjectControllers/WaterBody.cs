using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class WaterBody : AutoConfiguredMonoBehaviour {

        [SerializeField]
        private BoxCollider myCollider = null;

        [Header("Camera Fog")]

        [SerializeField]
        private bool useFog = true;

        [SerializeField]
        private Color fogColor = new Color32(0, 0, 255, 255);

        [SerializeField]
        [Range(0, 1)]
        private float fogDensity = 1f;

        [Header("Sound")]

        [Tooltip("A sound to play when a character enters the water.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string enterWaterAudioProfileName = string.Empty;

        [Tooltip("A looping sound to play while a character is swimming.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string swimLoopAudioProfileName = string.Empty;

        [Tooltip("A sound to play whenever a character splashes while swimming.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string swimHitsAudioProfileName = string.Empty;

        private AudioProfile enterWaterAudioProfile;
        private AudioProfile swimLoopAudioProfile;
        private AudioProfile swimHitsAudioProfile;


        // deduced settings
        private float surfaceHeight = 0f;

        // state tracking
        private bool fogActivated = false;

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private WeatherManager weatherManager = null;

        public BoxCollider Collider { get => myCollider; set => myCollider = value; }
        public float SurfaceHeight { get => surfaceHeight; set => surfaceHeight = value; }
        public AudioProfile EnterWaterAudioProfile { get => enterWaterAudioProfile;  }
        public AudioProfile SwimLoopAudioProfile { get => swimLoopAudioProfile; }
        public AudioProfile SwimHitsAudioProfile { get => swimHitsAudioProfile;  }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            surfaceHeight = myCollider.bounds.max.y;
            //Debug.Log("surfaceHeight = " + surfaceHeight);

            SetupScriptableObjects();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
            weatherManager = systemGameManager.WeatherManager;
        }

        private void OnTriggerEnter(Collider other) {
            //Debug.Log($"{gameObject.name}.Water.OnTriggerEnter(" + other.gameObject.name + ")");

            // configure camera
            if (useFog == true
                && other.tag == "MainCamera"
                && fogActivated == false) {
                fogActivated = true;

                // set overrides
                weatherManager.ActivateWaterFogSettings(true, fogColor, fogDensity);
            }

            // configure character
            UnitController unitController = other.gameObject.GetComponent<UnitController>();
            if (unitController != null) {
                unitController.EnterWater(this);
            }
        }


        private void OnTriggerExit(Collider other) {
            //Debug.Log($"{gameObject.name}.Water.OnTriggerExit(" + other.gameObject.name + ")");

            // configure camera
            if (useFog == true
                && other.tag == "MainCamera"
                && fogActivated == true) {
                fogActivated = false;

                // restore original settings
                weatherManager.DeactivateWaterFogSettings();
            }

            // configure character
            UnitController unitController = other.gameObject.GetComponent<UnitController>();
            if (unitController != null) {
                unitController.ExitWater(this);
            }

        }

        private void SetupScriptableObjects() {
            //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.SetupScriptableObjects()");
            if (systemGameManager == null) {
                Debug.LogError(gameObject.name + ": SystemGameManager not found.  Is the GameManager in the scene?");
                return;
            }

            if (enterWaterAudioProfileName != null && enterWaterAudioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = systemDataFactory.GetResource<AudioProfile>(enterWaterAudioProfileName);
                if (tmpAudioProfile != null) {
                    enterWaterAudioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("MovementSoundArea.SetupScriptableObjects(): Could not find audio profile : " + enterWaterAudioProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }

            if (swimLoopAudioProfileName != null && swimLoopAudioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = systemDataFactory.GetResource<AudioProfile>(swimLoopAudioProfileName);
                if (tmpAudioProfile != null) {
                    swimLoopAudioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("MovementSoundArea.SetupScriptableObjects(): Could not find audio profile : " + swimLoopAudioProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }

            if (swimHitsAudioProfileName != null && swimHitsAudioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = systemDataFactory.GetResource<AudioProfile>(swimHitsAudioProfileName);
                if (tmpAudioProfile != null) {
                    swimHitsAudioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("MovementSoundArea.SetupScriptableObjects(): Could not find audio profile : " + swimHitsAudioProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }

        }
    }

}
