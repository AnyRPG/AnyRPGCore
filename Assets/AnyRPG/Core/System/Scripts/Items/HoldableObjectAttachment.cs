using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class HoldableObjectAttachment : ConfiguredClass {

        [SerializeField]
        private List<AttachmentNode> attachmentNodes = new List<AttachmentNode>();

        public List<AttachmentNode> AttachmentNodes { get => attachmentNodes; set => attachmentNodes = value; }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            if (attachmentNodes != null) {
                foreach (AttachmentNode attachmentNode in attachmentNodes) {
                    if (attachmentNode != null) {
                        attachmentNode.SetupScriptableObjects(systemGameManager);
                    }
                }
            }
        }
    }
}

