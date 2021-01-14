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

        [Header("UMA Recipes")]

        [Tooltip("A list of UMA recipes to equip. Specify as many recipes for as many races as you want, and the ones that match the current race will be equipped.")]
        [SerializeField]
        private List<UMA.UMATextRecipe> uMARecipes = new List<UMATextRecipe>();

        public List<UMATextRecipe> MyUMARecipes { get => uMARecipes; set => uMARecipes = value; }
    }
}