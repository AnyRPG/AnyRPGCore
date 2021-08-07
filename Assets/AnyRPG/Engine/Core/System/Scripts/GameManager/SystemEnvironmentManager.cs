using AnyRPG;
using UnityEngine;


namespace AnyRPG {
    public class SystemEnvironmentManager : ConfiguredMonoBehaviour {

        public static void SetSkyBox(Material newMaterial) {
            RenderSettings.skybox = newMaterial;
        }
    }
}
