using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SwappableMeshModelOptions {

        [Tooltip("Groups of exclusive meshes that should not be active at the same time.")]
        [SerializeField]
        private List<SwappableMeshOptionGroup> meshGroups = new List<SwappableMeshOptionGroup>();

        [Tooltip("Default meshes to display when no option is chosen.")]
        [SerializeField]
        private List<SwappableMeshOptionDefaults> groupDefaults = new List<SwappableMeshOptionDefaults>();

        public List<SwappableMeshOptionGroup> MeshGroups { get => meshGroups; set => meshGroups = value; }
        public List<SwappableMeshOptionDefaults> GroupDefaults { get => groupDefaults; set => groupDefaults = value; }
    }

}

