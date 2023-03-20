using System.Collections.Generic;
using UnityEngine;
using UMA;

namespace AnyRPG {

    [System.Serializable]
    public class UMAEquipmentModelProperties {

        [Tooltip("A list of UMA recipes to equip. Specify as many recipes for as many races as you want, and the ones that match the current race will be equipped.")]
        [SerializeField]
        private List<UMA.UMATextRecipe> uMARecipes = new List<UMATextRecipe>();

        [SerializeField]
        private List<SharedColorNode> sharedColors = new List<SharedColorNode>();

        public List<UMATextRecipe> UMARecipes { get => uMARecipes; set => uMARecipes = value; }
        public List<SharedColorNode> SharedColors { get => sharedColors; set => sharedColors = value; }

    }

    [System.Serializable]
    public class SharedColorNode {

        [SerializeField]
        private string sharedColorname = string.Empty;

        [SerializeField]
        private Color color = new Color32(255, 255, 255, 255);

        public string SharedColorname { get => sharedColorname; set => sharedColorname = value; }
        public Color Color { get => color; set => color = value; }
    }
}