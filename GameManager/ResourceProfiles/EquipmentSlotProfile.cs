using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Equipment Slot", menuName = "AnyRPG/Equipment/EquipmentSlot")]
    [System.Serializable]
    public class EquipmentSlotProfile : DescribableResource {

        
        // a weighted value to control distribution of stats among gear
        [SerializeField]
        private float statWeight = 1;
        
        [SerializeField]
        private List<string> equipmentSlotTypeList = new List<string>();

        private List<EquipmentSlotType> realEquipmentSlotTypeList = new List<EquipmentSlotType>();

        public List<EquipmentSlotType> MyEquipmentSlotTypeList { get => realEquipmentSlotTypeList; set => realEquipmentSlotTypeList = value; }
        public float MyStatWeight { get => statWeight; set => statWeight = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            realEquipmentSlotTypeList = new List<EquipmentSlotType>();
            if (equipmentSlotTypeList != null) {
                foreach (string equipmentSlotTypeName in equipmentSlotTypeList) {
                    EquipmentSlotType tmpSlotType = SystemEquipmentSlotTypeManager.MyInstance.GetResource(equipmentSlotTypeName);
                    if (tmpSlotType != null) {
                        realEquipmentSlotTypeList.Add(tmpSlotType);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find equipmentSlotType: " + equipmentSlotTypeName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }

    }

}