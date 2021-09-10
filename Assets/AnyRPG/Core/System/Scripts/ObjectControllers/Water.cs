using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class Water : MonoBehaviour {

        [SerializeField]
        private BoxCollider myCollider = null;

        [SerializeField]
        private bool useFog = true;

        [SerializeField]
        private Color fogColor = new Color32(0, 0, 255, 255);

        [SerializeField]
        [Range(0, 1)]
        private float fogDensity = 1f;

        // save settings in case fog was originally activated in level
        private bool originalUseFog = false;
        private Color originalFogColor;
        private float originalFogDensity = 0f;

        private bool fogActivated = false;


        private void OnTriggerEnter(Collider other) {
            Debug.Log(gameObject.name + ".Water.OnTriggerEnter(" + other.gameObject.name + ")");
            if (useFog == true
                && other.tag == "MainCamera"
                && fogActivated == false) {
                fogActivated = true;

                // backup original settings
                originalFogColor = RenderSettings.fogColor;
                originalUseFog = RenderSettings.fog;
                originalFogDensity = RenderSettings.fogDensity;

                // set overrides
                RenderSettings.fog = true;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
            }
        }


        private void OnTriggerExit(Collider other) {
            Debug.Log(gameObject.name + ".Water.OnTriggerExit(" + other.gameObject.name + ")");
            if (useFog == true
                && other.tag == "MainCamera"
                && fogActivated == true) {
                fogActivated = false;

                // restore original settings
                RenderSettings.fog = originalUseFog;
                RenderSettings.fogColor = originalFogColor;
                RenderSettings.fogDensity = originalFogDensity;
            }

        }
    }

}
