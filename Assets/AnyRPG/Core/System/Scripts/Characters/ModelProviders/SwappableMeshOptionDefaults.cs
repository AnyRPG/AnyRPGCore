using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class SwappableMeshOptionDefaults {

        [Tooltip("The name of the option group.")]
        [SerializeField]
        private string groupName = string.Empty;

        [Tooltip("The name of the item in the prefab that contains the mesh renderer that will be enabled when the option group has no meshes equipped.")]
        [SerializeField]
        private string optionName = string.Empty;

        public string GroupName { get => groupName; set => groupName = value; }
        public string OptionName { get => optionName; set => optionName = value; }
    }

}

