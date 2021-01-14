using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class ProjectorColorMapNode {

        [SerializeField]
        private Color sourceColor = new Color32(255, 255, 255, 255);

        [SerializeField]
        private Material projectorMaterial;

        public Color MySourceColor { get => sourceColor; set => sourceColor = value; }
        public Material MyProjectorMaterial { get => projectorMaterial; set => projectorMaterial = value; }
    }

}