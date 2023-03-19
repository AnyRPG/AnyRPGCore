using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SwappableMeshEquipmentModel : EquipmentModel {

        [Header("Swappable Mesh Equipment Models")]

        [Tooltip("A list of option groups and option names as defined in the swappable mesh model profile")]
        [SerializeField]
        private List<SwappableMeshEquipmentModelNode> meshes = new List<SwappableMeshEquipmentModelNode>();

        public List<SwappableMeshEquipmentModelNode> Meshes { get => meshes; }
    }
  
}

