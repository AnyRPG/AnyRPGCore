using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SwappableMeshEquipmentModel : EquipmentModel {

        //[Header("Swappable Mesh Equipment Models")]

        [SerializeField]
        private SwappableMeshEquipmentModelProperties properties = new SwappableMeshEquipmentModelProperties();

        public SwappableMeshEquipmentModelProperties Properties { get => properties; set => properties = value; }
    }
  
}

