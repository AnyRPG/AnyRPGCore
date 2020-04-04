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

        [SerializeField]
        private List<UMA.UMATextRecipe> uMARecipes = new List<UMATextRecipe>();

        public UMATextRecipe MyUMARecipe { get => uMARecipe; set => uMARecipe = value; }
        public List<UMATextRecipe> MyUMARecipes { get => uMARecipes; set => uMARecipes = value; }
    }
}