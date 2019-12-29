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

        private PrefabProfile holdableObject = null;

        public EquipmentSlotProfile MyEquipmentSlotProfile { get => equipmentSlotProfile; set => equipmentSlotProfile = value; }
        public PrefabProfile MyHoldableObject { get => holdableObject; set => holdableObject = value; }

        public void SetupScriptableObjects() {
            holdableObject = null;
            if (holdableObjectName != null && holdableObjectName != string.Empty) {
                PrefabProfile tmpHoldableObject = SystemPrefabProfileManager.MyInstance.GetResource(holdableObjectName);
                if (tmpHoldableObject != null) {
                    holdableObject = tmpHoldableObject;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find holdable object : " + holdableObjectName + " while inititalizing an attachment node.  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): holdable object name blank while inititalizing an attachment node.  CHECK INSPECTOR");
            }
            equipmentSlotProfile = null;
            if (equipmentSlotProfileName != null && equipmentSlotProfileName != string.Empty) {
                EquipmentSlotProfile tmpEquipmentSlotProfile = SystemEquipmentSlotProfileManager.MyInstance.GetResource(equipmentSlotProfileName);
                if (tmpEquipmentSlotProfile != null) {
                    equipmentSlotProfile = tmpEquipmentSlotProfile;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find equipmentSlotProfile : " + equipmentSlotProfileName + " while inititalizing an attachment node.  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): equipmentSlotProfile name blank while inititalizing an attachment node.  CHECK INSPECTOR");
            }
        }
    }

}
