using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttachmentNode {

    [SerializeField]
    private string equipmentSlotProfileName;

    [SerializeField]
    private string holdableObjectName;

    public string MyEquipmentSlotProfileName { get => equipmentSlotProfileName; set => equipmentSlotProfileName = value; }
    public string MyHoldableObjectName { get => holdableObjectName; set => holdableObjectName = value; }
}
