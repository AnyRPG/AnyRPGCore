using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class RecipeScript : HighlightButton {

        private string recipeName;

        private Recipe recipe;

        public Recipe MyRecipe { get => recipe; set => recipe = value; }

        public void SetRecipeName(string recipeName) {
            if (recipeName != null && recipeName != string.Empty) {
                this.recipeName = recipeName;
            }
        }

        public override void Select() {
            //Debug.Log(gameObject.name + ".RecipeScript.Select(): " + recipeName);

            base.Select();
            CraftingUI.MyInstance.MySelectedRecipeScript = this;

            //GetComponent<Text>().color = Color.red;
            CraftingUI.MyInstance.ShowDescription(MyRecipe);

        }

        public override void DeSelect() {
            //Debug.Log("RecipeScript.DeSelect()");
            base.DeSelect();
        }

    }

}