using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New UMA Recipe Profile", menuName = "AnyRPG/Inventory/Equipment/Equipment Model Profile")]
    public class EquipmentModelProfile : DescribableResource {

        [Header("Equipment Model")]

        [FormerlySerializedAs("uMARecipeProfileProperties")]
        [SerializeField]
        private UMAEquipmentModelProperties deprecatedUMARecipeProfileProperties = new UMAEquipmentModelProperties();

        [SerializeField]
        private EquipmentModelProperties properties = new EquipmentModelProperties();


        //public UMARecipeProfileProperties Properties { get => uMARecipeProfileProperties; set => uMARecipeProfileProperties = value; }
        public EquipmentModelProperties Properties { get => properties; set => properties = value; }
        public UMAEquipmentModelProperties DeprecatedUMARecipeProfileProperties { get => deprecatedUMARecipeProfileProperties; set => deprecatedUMARecipeProfileProperties = value; }
    }
   
}