using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class RecipeScript : HighlightButton {

        //private string recipeName;

        private Recipe recipe;

        public Recipe Recipe { get => recipe; set => recipe = value; }

        public void SetRecipe(Recipe newRecipe) {
            //if (recipeName != null && recipeName != string.Empty) {
                recipe = newRecipe;
            //}
        }

        public override void Select() {
            //Debug.Log(gameObject.name + ".RecipeScript.Select(): " + (recipe == null ? "null" : recipe.DisplayName));

            base.Select();
            CraftingManager.Instance.SetSelectedRecipe(recipe);
        }

        public override void DeSelect() {
            //Debug.Log(gameObject.name + "RecipeScript.DeSelect(): " + (recipe == null ? "null" : recipe.DisplayName));
            base.DeSelect();
        }

    }

}