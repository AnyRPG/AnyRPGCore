using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SwappableMeshOptionGroup {

        [Tooltip("The name for this mesh group that should be shown in the character creator.")]
        [SerializeField]
        private string groupName = string.Empty;

        [Tooltip("If true, the player will not see this group in the character appearance editor.")]
        [SerializeField]
        private bool hidden = false;

        [Tooltip("If true, the player can choose to not display any meshes from this group.")]
        [SerializeField]
        private bool optional = false;

        [Tooltip("How this group options should be displayed to the player.")]
        [SerializeField]
        private SwappableMeshOptionGroupType displayAs = SwappableMeshOptionGroupType.List;

        [Tooltip("The names of the GameObjects that contain meshes in the model prefab.")]
        [SerializeField]
        private List<SwappableMeshOptionChoice> meshes = new List<SwappableMeshOptionChoice>();

        public string GroupName { get => groupName; set => groupName = value; }
        public bool Optional { get => optional; set => optional = value; }
        public List<SwappableMeshOptionChoice> Meshes { get => meshes; set => meshes = value; }
        public bool Hidden { get => hidden; set => hidden = value; }
        public SwappableMeshOptionGroupType DisplayAs { get => displayAs; set => displayAs = value; }
    }

    public enum SwappableMeshOptionGroupType { List, Grid }

}

