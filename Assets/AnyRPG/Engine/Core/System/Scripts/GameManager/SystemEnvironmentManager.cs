using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class SystemEnvironmentManager : MonoBehaviour {

        #region Singleton
        private static SystemEnvironmentManager instance;

        public static SystemEnvironmentManager Instance {
            get {
                return instance;
            }
        }

        private void Awake() {
            instance = this;
        }
        #endregion

        public void SetSkyBox(Material newMaterial) {
            RenderSettings.skybox = newMaterial;
        }
    }
}
