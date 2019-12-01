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

        public List<string> MyExclusiveSlotProfileList { get => exclusiveSlotProfileList; set => exclusiveSlotProfileList = value; }
        public float MyStatWeight { get => statWeight; set => statWeight = value; }

        public List<EquipmentSlotProfile> GetCompatibleSlotProfiles() {
            List<EquipmentSlotProfile> returnValue = new List<EquipmentSlotProfile>();
            if (MyName != null && MyName != string.Empty) {
                foreach (EquipmentSlotProfile equipmentSlotProfile in SystemEquipmentSlotProfileManager.MyInstance.MyResourceList.Values) {
                    if (equipmentSlotProfile.MyEquipmentSlotTypeList != null && equipmentSlotProfile.MyEquipmentSlotTypeList.Contains(MyName)) {
                        returnValue.Add(equipmentSlotProfile);
                    }
                }
            }
            return returnValue;
        }
    }

}