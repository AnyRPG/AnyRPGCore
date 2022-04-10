using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class AttachmentNode : ConfiguredClass {

        [Tooltip("Depending on the equipment slot type (eg one hand), this object may be assigned to different actual slots (main hand vs off hand).  This relationship defines what holdable object to use for what actual slot.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(EquipmentSlotProfile))]
        private string equipmentSlotProfileName = string.Empty;

        private EquipmentSlotProfile equipmentSlotProfile = null;

        [Tooltip("The name of the holdable object profile that refers to the physical prefab")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(PrefabProfile))]
        private string holdableObjectName = string.Empty;

        private PrefabProfile holdableObject = null;

        [Tooltip("If true, the item will search the prefabProfile of the unit this is being equippped on for the following attachment points")]
        [SerializeField]
        private bool useUniversalAttachment = false;

        [Tooltip("The name of the attachment to use when this item is sheathed (or worn in the case of armor)")]
        [SerializeField]
        private string primaryAttachmentName = string.Empty;

        [Tooltip("The name of the attachment to use when this item is unSheathed")]
        [SerializeField]
        private string unsheathedAttachmentName = string.Empty;

        public EquipmentSlotProfile MyEquipmentSlotProfile { get => equipmentSlotProfile; set => equipmentSlotProfile = value; }
        public PrefabProfile HoldableObject { get => holdableObject; set => holdableObject = value; }
        public bool UseUniversalAttachment { get => useUniversalAttachment; set => useUniversalAttachment = value; }
        public string PrimaryAttachmentName { get => primaryAttachmentName; set => primaryAttachmentName = value; }
        public string UnsheathedAttachmentName { get => unsheathedAttachmentName; set => unsheathedAttachmentName = value; }
        public string HoldableObjectName { get => holdableObjectName; set => holdableObjectName = value; }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            holdableObject = null;
            if (holdableObjectName != null && holdableObjectName != string.Empty) {
                PrefabProfile tmpHoldableObject = systemDataFactory.GetResource<PrefabProfile>(holdableObjectName);
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
                EquipmentSlotProfile tmpEquipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
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
