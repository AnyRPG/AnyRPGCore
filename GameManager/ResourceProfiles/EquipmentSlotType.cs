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

        [SerializeField]
        private List<string> exclusiveSlotProfileList = new List<string>();

        public List<string> MyExclusiveSlotProfileList { get => exclusiveSlotProfileList; set => exclusiveSlotProfileList = value; }
    }

}