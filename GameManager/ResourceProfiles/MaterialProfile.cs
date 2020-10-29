using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Material Profile", menuName = "AnyRPG/MaterialProfile")]
    public class MaterialProfile : DescribableResource {

        [Header("Material")]

        [Tooltip("a material to temporarily assign to the target we hit")]
        [SerializeField]
        private Material effectMaterial;

        public Material MyEffectMaterial { get => effectMaterial; set => effectMaterial = value; }
    }
}