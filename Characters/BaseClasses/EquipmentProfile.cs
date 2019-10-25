using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New Equipment Profile", menuName = "Equipment Profile")]
[System.Serializable]
public class EquipmentProfile : DescribableResource {

    [SerializeField]
    private List<string> equipmentNameList = new List<string>();

    public List<string> MyEquipmentNameList { get => equipmentNameList; set => equipmentNameList = value; }
}

}