using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Environment State", menuName = "AnyRPG/EnvironmentState")]
    [System.Serializable]
    public class EnvironmentStateProfile : DescribableResource {

        [Header("Environment State")]
        
        [Tooltip("The name of the material profile that contains the skybox that should be used when this environment state is active")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(MaterialProfile))]
        private string skyBoxMaterialProfileName = string.Empty;

        // reference to the actual material
        private Material skyBoxMaterial;

        public Material MySkyBoxMaterial { get => skyBoxMaterial; set => skyBoxMaterial = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (skyBoxMaterialProfileName != null && skyBoxMaterialProfileName != string.Empty) {
                MaterialProfile tmpMaterialProfile = SystemDataFactory.Instance.GetResource<MaterialProfile>(skyBoxMaterialProfileName);
                if (tmpMaterialProfile != null && tmpMaterialProfile.MyEffectMaterial != null) {
                    skyBoxMaterial = tmpMaterialProfile.MyEffectMaterial;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find material profile : " + skyBoxMaterialProfileName + " or its material while inititalizing " + name + ".  CHECK INSPECTOR");
                }

            }
        }
    }

}