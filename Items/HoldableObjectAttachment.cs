using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HoldableObjectAttachment {

    [SerializeField]
    private List<AttachmentNode> attachmentNodes = new List<AttachmentNode>();

    public List<AttachmentNode> MyAttachmentNodes { get => attachmentNodes; set => attachmentNodes = value; }
}
