﻿using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class AbilityAttachmentNode : ConfiguredClass {

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
        private string attachmentName = string.Empty;

        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public PrefabProfile HoldableObject { get => holdableObject; set => holdableObject = value; }
        public bool UseUniversalAttachment { get => useUniversalAttachment; set => useUniversalAttachment = value; }
        public string AttachmentName { get => attachmentName; set => attachmentName = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void SetupScriptableObjects(string ownerName, SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            holdableObject = null;
            if (holdableObjectName != null && holdableObjectName != string.Empty) {
                PrefabProfile tmpHoldableObject = systemDataFactory.GetResource<PrefabProfile>(holdableObjectName);
                if (tmpHoldableObject != null) {
                    holdableObject = tmpHoldableObject;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find holdable object : " + holdableObjectName + " while inititalizing an attachment node for " + ownerName + ".  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): holdable object name blank while inititalizing an attachment node.  CHECK INSPECTOR");
            }
        }
    }

}
