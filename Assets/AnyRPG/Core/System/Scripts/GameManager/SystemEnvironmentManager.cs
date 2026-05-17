using AnyRPG;
using UnityEngine;


namespace AnyRPG {
    public class SystemEnvironmentManager : ConfiguredClass {

        public static void SetSkyBox(Material newMaterial) {
            RenderSettings.skybox = newMaterial;
        }
    }
}
