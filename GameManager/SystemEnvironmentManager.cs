using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class SystemEnvironmentManager : MonoBehaviour {

        #region Singleton
        private static SystemEnvironmentManager instance;

        public static SystemEnvironmentManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemEnvironmentManager>();
                }

                return instance;
            }
        }
        #endregion

        public void SetSkyBox(Material newMaterial) {
            RenderSettings.skybox = newMaterial;
        }
    }
}
