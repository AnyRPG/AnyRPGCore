using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class AbilityAttachmentNode {

        [Tooltip("The name of the holdable object profile that refers to the physical prefab")]
        [SerializeField]
        private string holdableObjectName = string.Empty;

        private PrefabProfile holdableObject = null;

        [Tooltip("If true, the item will search the prefabProfile of the unit this is being equippped on for the following attachment points")]
        [SerializeField]
        private bool useUniversalAttachment = false;

        [Tooltip("The name of the attachment to use when this item is sheathed (or worn in the case of armor)")]
        [SerializeField]
        private string attachmentName = string.Empty;

        public PrefabProfile HoldableObject { get => holdableObject; set => holdableObject = value; }
        public bool UseUniversalAttachment { get => useUniversalAttachment; set => useUniversalAttachment = value; }
        public string AttachmentName { get => attachmentName; set => attachmentName = value; }

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
        }
    }

}
