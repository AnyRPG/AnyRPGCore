using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class EnvironmentPreviewProperties {
        [Tooltip("A prefab to instantiate in the new game window.")]
        [SerializeField]
        private GameObject environmentPreviewPrefab = null;

        [Tooltip("The material on the platform the player stands on in the new game window.")]
        [SerializeField]
        private Material platformMaterial = null;

        [Tooltip("The skybox material on the top quad in the new game window.")]
        [SerializeField]
        private Material topMaterial = null;

        [Tooltip("The skybox material on the bottom quad in the new game window.")]
        [SerializeField]
        private Material bottomMaterial = null;

        [Tooltip("The skybox material on the north quad in the new game window.")]
        [SerializeField]
        private Material northMaterial = null;

        [Tooltip("The skybox material on the south quad in the new game window.")]
        [SerializeField]
        private Material southMaterial = null;

        [Tooltip("The skybox material on the east quad in the new game window.")]
        [SerializeField]
        private Material eastMaterial = null;

        [Tooltip("The skybox material on the west quad in the new game window.")]
        [SerializeField]
        private Material westMaterial = null;

        public GameObject EnvironmentPreviewPrefab { get => environmentPreviewPrefab; set => environmentPreviewPrefab = value; }
        public Material PlatformMaterial { get => platformMaterial; set => platformMaterial = value; }
        public Material TopMaterial { get => topMaterial; set => topMaterial = value; }
        public Material BottomMaterial { get => bottomMaterial; set => bottomMaterial = value; }
        public Material NorthMaterial { get => northMaterial; set => northMaterial = value; }
        public Material SouthMaterial { get => southMaterial; set => southMaterial = value; }
        public Material EastMaterial { get => eastMaterial; set => eastMaterial = value; }
        public Material WestMaterial { get => westMaterial; set => westMaterial = value; }
    }

}
