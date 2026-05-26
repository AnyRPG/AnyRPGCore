using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class FogSettings {

        [Tooltip("If true, fog will be turned on.")]
        [SerializeField]
        private bool useFog = false;

        [Tooltip("The color of the fog.")]
        [SerializeField]
        private Color fogColor = new Color32(128, 128, 128, 255);

        [Tooltip("The density of the fog.")]
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