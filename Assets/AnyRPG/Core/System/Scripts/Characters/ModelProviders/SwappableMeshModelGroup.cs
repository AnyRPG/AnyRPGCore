using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SwappableMeshModelGroup {

        [Tooltip("The name for this mesh group that should be shown in the character creator.")]
        [SerializeField]
        private string groupName = string.Empty;

        [Tooltip("If true, the player can choose to not display any meshes from this group.")]
        [SerializeField]
        private bool optional = false;

        [Tooltip("The names of the GameObjects that contain meshes in the model prefab.")]
        [SerializeField]
        private List<string> meshNames = new List<string>();

        public string GroupName { get => groupName; set => groupName = value; }
        public List<string> MeshNames { get => meshNames; set => meshNames = value; }
        public bool Optional { get => optional; set => optional = value; }
    }

}

