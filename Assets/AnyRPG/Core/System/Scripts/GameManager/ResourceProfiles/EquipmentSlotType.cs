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
        [FormerlySerializedAs("exclusiveSlotProfileList")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(EquipmentSlotProfile))]
        private List<string> exclusiveSlotProfiles = new List<string>();

        [Tooltip("If this slot type takes up more than one physical slots, the slot types listed below will be unequipped to make room for the item in this slot")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(EquipmentSlotType))]
        private List<string> exclusiveSlotTypes = new List<string>();

        private List<EquipmentSlotProfile> exclusiveSlotProfileList = new List<EquipmentSlotProfile>();
        private List<EquipmentSlotType> exclusiveSlotTypeList = new List<EquipmentSlotType>();

        public float StatWeight { get => statWeight; set => statWeight = value; }
        public List<EquipmentSlotProfile> ExclusiveSlotProfileList { get => exclusiveSlotProfileList; set => exclusiveSlotProfileList = value; }
        public List<EquipmentSlotType> ExclusiveSlotTypeList { get => exclusiveSlotTypeList; set => exclusiveSlotTypeList = value; }

        public List<EquipmentSlotProfile> GetCompatibleSlotProfiles() {
            List<EquipmentSlotProfile> returnValue = new List<EquipmentSlotProfile>();
            foreach (EquipmentSlotProfile equipmentSlotProfile in systemDataFactory.GetResourceList<EquipmentSlotProfile>()) {
                if (equipmentSlotProfile.EquipmentSlotTypeList != null && equipmentSlotProfile.EquipmentSlotTypeList.Contains(this)) {
                    returnValue.Add(equipmentSlotProfile);
                }
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            exclusiveSlotProfileList = new List<EquipmentSlotProfile>();
            if (exclusiveSlotProfiles != null) {
                foreach (string exclusiveSlotProfile in exclusiveSlotProfiles) {
                    EquipmentSlotProfile tmpSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(exclusiveSlotProfile);
                    if (tmpSlotProfile != null) {
                        exclusiveSlotProfileList.Add(tmpSlotProfile);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find realExclusiveSlotProfile: " + exclusiveSlotProfile + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            exclusiveSlotTypeList = new List<EquipmentSlotType>();
            if (exclusiveSlotTypes != null) {
                foreach (string exclusiveSlotType in exclusiveSlotTypes) {
                    EquipmentSlotType tmpSlotType = systemDataFactory.GetResource<EquipmentSlotType>(exclusiveSlotType);
                    if (tmpSlotType != null) {
                        exclusiveSlotTypeList.Add(tmpSlotType);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find exclusiveSlotType: " + exclusiveSlotType + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }
    }

}