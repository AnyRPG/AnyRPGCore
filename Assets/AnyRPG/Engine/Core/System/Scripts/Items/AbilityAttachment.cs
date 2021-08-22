using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class AbilityAttachment {

        [SerializeField]
        private List<AbilityAttachmentNode> attachmentNodes = new List<AbilityAttachmentNode>();

        public List<AbilityAttachmentNode> MyAttachmentNodes { get => attachmentNodes; set => attachmentNodes = value; }

        public void SetupScriptableObjects(string ownerName, SystemGameManager systemGameManager) {
            if (attachmentNodes != null) {
                foreach (AbilityAttachmentNode attachmentNode in attachmentNodes) {
                    if (attachmentNode != null) {
                        attachmentNode.SetupScriptableObjects(ownerName, systemGameManager);
                    }
                }
            }
        }
    }
}

