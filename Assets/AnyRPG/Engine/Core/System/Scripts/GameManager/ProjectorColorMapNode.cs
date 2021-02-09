using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class ProjectorColorMapNode {

        [SerializeField]
        private Color sourceColor = new Color32(255, 255, 255, 255);

        [Tooltip("The material that should be used when a highlight circle matches the color")]
        [SerializeField]
        private Material projectorMaterial;

        [Tooltip("If true, the material will also be tinted with the color")]
        [SerializeField]
        private bool tintMaterial = true;

        public Color SourceColor { get => sourceColor; set => sourceColor = value; }
        public Material ProjectorMaterial { get => projectorMaterial; set => projectorMaterial = value; }
        public bool TintMaterial { get => tintMaterial; set => tintMaterial = value; }
    }

}