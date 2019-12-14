using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class AttachmentNode {

        [SerializeField]
        private string equipmentSlotProfileName;

        private EquipmentSlotProfile equipmentSlotProfile = null;

        [SerializeField]
        private string holdableObjectName;

        private HoldableObject holdableObject = null;

        public EquipmentSlotProfile MyEquipmentSlotProfile { get => equipmentSlotProfile; set => equipmentSlotProfile = value; }
        public HoldableObject MyHoldableObject { get => holdableObject; set => holdableObject = value; }

        public void SetupScriptableObjects() {
            holdableObject = null;
            if (holdableObjectName != null && holdableObjectName != string.Empty) {
                holdableObject = SystemHoldableObjectManager.MyInstance.GetResource(holdableObjectName);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find holdable object : " + holdableObjectName + " while inititalizing an attachment node.  CHECK INSPECTOR");
            }
            equipmentSlotProfile = null;
            if (equipmentSlotProfileName != null && equipmentSlotProfileName != string.Empty) {
                equipmentSlotProfile = SystemEquipmentSlotProfileManager.MyInstance.GetResource(equipmentSlotProfileName);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find equipmentSlotProfile : " + equipmentSlotProfileName + " while inititalizing an attachment node.  CHECK INSPECTOR");
            }
        }
    }

}
