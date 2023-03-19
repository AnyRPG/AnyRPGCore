using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SwappableMeshEquipmentModelNode {

        [Tooltip("The name of the option group.")]
        [SerializeField]
        private string groupName = string.Empty;

        [Tooltip("The name of the option choice (not the mesh name).")]
        [SerializeField]
        private string optionName = string.Empty;

        public string GroupName { get => groupName; set => groupName = value; }
        public string OptionName { get => optionName; set => optionName = value; }
    }
}

