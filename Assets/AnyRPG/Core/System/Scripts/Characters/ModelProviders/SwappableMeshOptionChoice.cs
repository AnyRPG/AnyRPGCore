using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class SwappableMeshOptionChoice {

        [Tooltip("The name that should be displayed for this option choice.")]
        [SerializeField]
        private string displayName = string.Empty;

        [Tooltip("An image to be displayed for this option choice.")]
        [SerializeField]
        private Sprite icon = null;

        [Tooltip("The name of the item in the prefab that contains the mesh renderer that will be enabled when this option is chosen.")]
        [SerializeField]
        private string meshName = string.Empty;

        public string DisplayName { get => displayName; set => displayName = value; }
        public Sprite Icon { get => icon; set => icon = value; }
        public string MeshName { get => meshName; set => meshName = value; }
    }

}

