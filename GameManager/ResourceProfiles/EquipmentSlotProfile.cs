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

        [SerializeField]
        private List<string> equipmentSlotTypeList = new List<string>();

        public List<string> MyEquipmentSlotTypeList { get => equipmentSlotTypeList; set => equipmentSlotTypeList = value; }
    }

}