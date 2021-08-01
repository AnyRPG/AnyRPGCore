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

        [Header("Equipment Slot Type")]

        [Tooltip("a weighted value to control distribution of stats among gear")]
        [SerializeField]
        private float statWeight = 1f;

        [Header("Exclusivity")]

        [Tooltip("If this slot type takes up more than one physical slots, the slots listed below will be unequipped to make room for the item in this slot")]
        [SerializeField]
        private List<string> exclusiveSlotProfileList = new List<string>();

        private List<EquipmentSlotProfile> realExclusiveSlotProfileList = new List<EquipmentSlotProfile>();

        public List<EquipmentSlotProfile> MyExclusiveSlotProfileList { get => realExclusiveSlotProfileList; set => realExclusiveSlotProfileList = value; }
        public float MyStatWeight { get => statWeight; set => statWeight = value; }

        public List<EquipmentSlotProfile> GetCompatibleSlotProfiles() {
            List<EquipmentSlotProfile> returnValue = new List<EquipmentSlotProfile>();
            foreach (EquipmentSlotProfile equipmentSlotProfile in SystemDataFactory.Instance.GetResourceList<EquipmentSlotProfile>()) {
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
                    EquipmentSlotProfile tmpSlotProfile = SystemDataFactory.Instance.GetResource<EquipmentSlotProfile>(exclusiveSlotProfile);
                    if (tmpSlotProfile != null) {
                        realExclusiveSlotProfileList.Add(tmpSlotProfile);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find realExclusiveSlotProfile: " + exclusiveSlotProfile + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }
    }

}