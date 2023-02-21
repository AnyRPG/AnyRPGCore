using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SwappableMeshModelOptions {

        [Tooltip("Groups of exclusive meshes that should not be active at the same time.")]
        [SerializeField]
        private List<SwappableMeshModelGroup> meshGroups = new List<SwappableMeshModelGroup>();

        public List<SwappableMeshModelGroup> MeshGroups { get => meshGroups; set => meshGroups = value; }
    }

}

