using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UMA;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New UMA Recipe Profile", menuName = "AnyRPG/UMARecipeProfile")]
    public class UMARecipeProfile : DescribableResource {

        [SerializeField]
        private UMA.UMATextRecipe uMARecipe = null;

        public UMATextRecipe MyUMARecipe { get => uMARecipe; set => uMARecipe = value; }
    }
}