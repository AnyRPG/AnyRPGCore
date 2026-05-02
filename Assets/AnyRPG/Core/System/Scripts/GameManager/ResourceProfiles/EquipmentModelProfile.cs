using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Equipment Model Profile", menuName = "AnyRPG/Inventory/Equipment/Equipment Model Profile")]
    public class EquipmentModelProfile : DescribableResource {

        [Header("Equipment Model")]

        [SerializeField]
        private EquipmentModelProperties properties = new EquipmentModelProperties();

        public EquipmentModelProperties Properties { get => properties; set => properties = value; }
    }
   
}