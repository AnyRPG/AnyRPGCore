using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class RecipeScript : HighlightButton {

        protected Recipe recipe;

        // game manager references
        protected CraftingManager craftingManager = null;

        public Recipe Recipe { get => recipe; set => recipe = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

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