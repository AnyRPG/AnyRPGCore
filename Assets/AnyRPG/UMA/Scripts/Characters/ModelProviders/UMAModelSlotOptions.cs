using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class UMAModelSlotOptions : ConfiguredClass {

        [Tooltip("The name of the UMA wardrobe slot this list applies to.")]
        [SerializeField]
        private string slotName = string.Empty;

        [Tooltip("Allow = only the recipes below will be displayed. Deny = only the recipes below will not be displayed.")]
        [SerializeField]
        private UMARecipetListType recipeListType = UMARecipetListType.Deny;

        [Tooltip("The names of UMA recipes to be allowed or denied when displaying recipes in the new game panel and character creator.")]
        [SerializeField]
        private List<string> recipeNames = new List<string>();

        public string SlotName { get => slotName; set => slotName = value; }
        public List<string> RecipeNames { get => recipeNames; set => recipeNames = value; }
        public UMARecipetListType RecipeListType { get => recipeListType; set => recipeListType = value; }
    }

    public enum UMARecipetListType { Deny, Allow };

}

