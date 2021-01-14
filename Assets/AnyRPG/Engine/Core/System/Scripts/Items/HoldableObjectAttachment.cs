using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class HoldableObjectAttachment {

        [SerializeField]
        private List<AttachmentNode> attachmentNodes = new List<AttachmentNode>();

        public List<AttachmentNode> MyAttachmentNodes { get => attachmentNodes; set => attachmentNodes = value; }

        public void SetupScriptableObjects() {
            if (attachmentNodes != null) {
                foreach (AttachmentNode attachmentNode in attachmentNodes) {
                    if (attachmentNode != null) {
                        attachmentNode.SetupScriptableObjects();
                    }
                }
            }
        }
    }
}

