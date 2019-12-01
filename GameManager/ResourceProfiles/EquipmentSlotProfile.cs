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

        public List<string> MyEquipmentSlotTypeList { get => equipmentSlotTypeList; set => equipmentSlotTypeList = value; }
        public float MyStatWeight { get => statWeight; set => statWeight = value; }
    }

}