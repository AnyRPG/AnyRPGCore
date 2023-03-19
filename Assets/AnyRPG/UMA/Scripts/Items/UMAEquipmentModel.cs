using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class UMAEquipmentModel : EquipmentModel {

        [Header("UMA Equipment Models")]

        [Tooltip("The name of an UMA recipe to manually search for")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UMARecipeProfile))]
        private string umaRecipeProfileName = string.Empty;

        [Tooltip("Inline UMA recipe profile properties")]
        [SerializeField]
        private UMARecipeProfileProperties uMARecipeProfileProperties = new UMARecipeProfileProperties();

        public override void SetupScriptableObjects(IDescribable describable) {
            base.SetupScriptableObjects(describable);

            if (umaRecipeProfileName != null && umaRecipeProfileName != string.Empty) {
                UMARecipeProfile tmpUMARecipeProfile = systemDataFactory.GetResource<UMARecipeProfile>(umaRecipeProfileName);
                if (tmpUMARecipeProfile != null) {
                    uMARecipeProfileProperties = tmpUMARecipeProfile.Properties;
                } else {
                    Debug.LogError("UMAEquipmentModel.SetupScriptableObjects(): Could not find uma recipe profile : " + umaRecipeProfileName + " while inititalizing " + describable.ResourceName + ".  CHECK INSPECTOR");
                }
            }
        }

    }
}

