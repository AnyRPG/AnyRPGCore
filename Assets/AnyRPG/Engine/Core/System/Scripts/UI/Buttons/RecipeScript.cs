using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class RecipeScript : HighlightButton {

        private Recipe recipe;

        // game manager references
        private CraftingManager craftingManager = null;

        public Recipe Recipe { get => recipe; set => recipe = value; }

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            craftingManager = systemGameManager.CraftingManager;
        }

        public void SetRecipe(Recipe newRecipe) {
            recipe = newRecipe;
        }

        public override void Select() {
            //Debug.Log(gameObject.name + ".RecipeScript.Select(): " + (recipe == null ? "null" : recipe.DisplayName));

            base.Select();
            craftingManager.SetSelectedRecipe(recipe);
        }

        public override void DeSelect() {
            //Debug.Log(gameObject.name + "RecipeScript.DeSelect(): " + (recipe == null ? "null" : recipe.DisplayName));
            base.DeSelect();
        }

    }

}