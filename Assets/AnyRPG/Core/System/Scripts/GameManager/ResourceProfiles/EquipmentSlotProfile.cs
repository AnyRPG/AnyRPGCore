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

        [Header("Equipment Slot")]

        [Tooltip("if true, the icon from any item equipped in this slot will be used for auto-attack abilities on the action bars")]
        [SerializeField]
        private bool mainWeaponSlot = false;

        [Tooltip("a weighted value to control distribution of stats among gear")]
        [SerializeField]
        private float statWeight = 1;

        [Tooltip("Names of equipment slot types that can be equippped in this slot")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(EquipmentSlotType))]
        private List<string> equipmentSlotTypeList = new List<string>();

        [Header("Audio")]

        [Tooltip("If true, items in this slot will set on hit audio sounds if they have one.")]
        [SerializeField]
        private bool setOnHitAudio = false;

        private List<EquipmentSlotType> realEquipmentSlotTypeList = new List<EquipmentSlotType>();

        public List<EquipmentSlotType> MyEquipmentSlotTypeList { get => realEquipmentSlotTypeList; set => realEquipmentSlotTypeList = value; }
        public float MyStatWeight { get => statWeight; set => statWeight = value; }

        /// <summary>
        /// If true, items in this slot will set on hit audio sounds if they have one
        /// </summary>
        public bool SetOnHitAudio { get => setOnHitAudio; set => setOnHitAudio = value; }
        public bool MainWeaponSlot { get => mainWeaponSlot; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            realEquipmentSlotTypeList = new List<EquipmentSlotType>();
            if (equipmentSlotTypeList != null) {
                foreach (string equipmentSlotTypeName in equipmentSlotTypeList) {
                    EquipmentSlotType tmpSlotType = systemDataFactory.GetResource<EquipmentSlotType>(equipmentSlotTypeName);
                    if (tmpSlotType != null) {
                        realEquipmentSlotTypeList.Add(tmpSlotType);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find equipmentSlotType: " + equipmentSlotTypeName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }

    }

}