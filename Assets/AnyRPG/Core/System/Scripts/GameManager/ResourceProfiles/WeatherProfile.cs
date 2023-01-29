using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Weather Profile", menuName = "AnyRPG/WeatherProfile")]
    [System.Serializable]
    public class WeatherProfile : DescribableResource {

        [Header("Weather")]

        [Tooltip("A prefab profile containing a weather prefab to spawn.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(PrefabProfile))]
        private string prefabProfile = string.Empty;

        private PrefabProfile prefabProfileReference = null;

        [Tooltip("If true, regular day/night ambient sounds will not be played while this weather effect is active.")]
        [SerializeField]
        private bool suppressAmbientSounds = false;

        [Tooltip("Ambient sounds to play in the background while this weather is active. These will override any normal ambient sounds for the scene.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string ambientSoundsProfile = string.Empty;

        [Tooltip("Ambient sounds to play in the background while this weather is active.  This will override any audio profile chosen as well as any normal ambient sounds for the scene.")]
        [SerializeField]
        private AudioClip ambientSoundsAudio = null;

        private AudioProfile ambientSoundsProfileReference;

        [Tooltip("This number will be multiplied by the default shadow strength of the sun to give a relative shadow strength while the weather is active.")]
        [Range(0, 1)]
        [SerializeField]
        private float shadowStrength = 1f;

        [Tooltip("Settings that will override the default environmental fog settings for the scene.")]
        [SerializeField]
        private FogSettings fogSettings;


        public PrefabProfile PrefabProfile { get => prefabProfileReference; set => prefabProfileReference = value; }
        public bool SuppressAmbientSounds { get => suppressAmbientSounds; set => suppressAmbientSounds = value; }
        public AudioClip AmbientSound {
            get {
                if (ambientSoundsAudio != null) {
                    return ambientSoundsAudio;
                }
                if (ambientSoundsProfileReference != null) {
                    return ambientSoundsProfileReference.RandomAudioClip;
                }
                return null;
            }
        }

        public FogSettings FogSettings { get => fogSettings; set => fogSettings = value; }
        public float ShadowStrength { get => shadowStrength; set => shadowStrength = value; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (prefabProfile != null && prefabProfile != string.Empty) {
                PrefabProfile tmpPrefabProfile = systemDataFactory.GetResource<PrefabProfile>(prefabProfile);
                if (tmpPrefabProfile != null) {
                    prefabProfileReference = tmpPrefabProfile;
                } else {
                    Debug.LogError("WeatherProfile.SetupScriptableObjects(): Could not find prefab profile : " + prefabProfile + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }
            }

            ambientSoundsProfileReference = null;
            if (ambientSoundsProfile != null && ambientSoundsProfile != string.Empty) {
                AudioProfile tmpAmbientMusicProfile = systemDataFactory.GetResource<AudioProfile>(ambientSoundsProfile);
                if (tmpAmbientMusicProfile != null) {
                    ambientSoundsProfileReference = tmpAmbientMusicProfile;
                } else {
                    Debug.LogError("WeatherProfile.SetupScriptableObjects(): Could not find audio profile : " + ambientSoundsProfile + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

        }
    }

}