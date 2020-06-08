using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Equipment Slot Type", menuName = "AnyRPG/Equipment/EquipmentSlotType")]
    [System.Serializable]
    public class EquipmentSlotType : DescribableResource {

        // a weighted value to control distribution of stats among gear
        [SerializeField]
        private float statWeight = 1f;

        [SerializeField]
        private List<string> exclusiveSlotProfileList = new List<string>();

        private List<EquipmentSlotProfile> realExclusiveSlotProfileList = new List<EquipmentSlotProfile>();

        public List<EquipmentSlotProfile> MyExclusiveSlotProfileList { get => realExclusiveSlotProfileList; set => realExclusiveSlotProfileList = value; }
        public float MyStatWeight { get => statWeight; set => statWeight = value; }

        public List<EquipmentSlotProfile> GetCompatibleSlotProfiles() {
            List<EquipmentSlotProfile> returnValue = new List<EquipmentSlotProfile>();
            foreach (EquipmentSlotProfile equipmentSlotProfile in SystemEquipmentSlotProfileManager.MyInstance.MyResourceList.Values) {
                if (equipmentSlotProfile.MyEquipmentSlotTypeList != null && equipmentSlotProfile.MyEquipmentSlotTypeList.Contains(this)) {
                    returnValue.Add(equipmentSlotProfile);
                }
            }
            return returnValue;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            realExclusiveSlotProfileList = new List<EquipmentSlotProfile>();
            if (exclusiveSlotProfileList != null) {
                foreach (string exclusiveSlotProfile in exclusiveSlotProfileList) {
                    EquipmentSlotProfile tmpSlotProfile = SystemEquipmentSlotProfileManager.MyInstance.GetResource(exclusiveSlotProfile);
                    if (tmpSlotProfile != null) {
                        realExclusiveSlotProfileList.Add(tmpSlotProfile);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find realExclusiveSlotProfile: " + exclusiveSlotProfile + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }
    }

}