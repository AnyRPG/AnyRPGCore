using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UMA;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Material Profile", menuName = "AnyRPG/MaterialProfile")]
    public class MaterialProfile : DescribableResource {

        // a material to temporarily assign to the target we hit
        [SerializeField]
        private Material effectMaterial;

        public Material MyEffectMaterial { get => effectMaterial; set => effectMaterial = value; }
    }
}