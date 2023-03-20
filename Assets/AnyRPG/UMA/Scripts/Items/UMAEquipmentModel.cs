using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class UMAEquipmentModel : EquipmentModel {

        //[Header("UMA Equipment Models")]

        [SerializeField]
        private UMAEquipmentModelProperties properties = new UMAEquipmentModelProperties();

        public UMAEquipmentModelProperties Properties { get => properties; set => properties = value; }

        public override void SetupScriptableObjects(IDescribable describable) {
            base.SetupScriptableObjects(describable);

            /*
            if (umaRecipeProfileName != null && umaRecipeProfileName != string.Empty) {
                EquipmentModelProfile tmpUMARecipeProfile = systemDataFactory.GetResource<EquipmentModelProfile>(umaRecipeProfileName);
                if (tmpUMARecipeProfile != null) {
                    uMARecipeProfileProperties = tmpUMARecipeProfile.Properties;
                } else {
                    Debug.LogError("UMAEquipmentModel.SetupScriptableObjects(): Could not find uma recipe profile : " + umaRecipeProfileName + " while inititalizing " + describable.ResourceName + ".  CHECK INSPECTOR");
                }
            }
            */
        }

    }
}

