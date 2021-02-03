using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "RecipeItem", menuName = "AnyRPG/Inventory/Items/RecipeItem", order = 1)]
    public class RecipeItem : Item, IUseable {

        [Header("Recipe Item")]

        [Tooltip("The power resource to refill when this potion is used")]
        [SerializeField]
        private string recipeName = string.Empty;

        private Recipe recipe = null;

        public override bool Use() {
            //Debug.Log(MyDisplayName + ".RecipeItem.Use()");
            if (!PlayerManager.MyInstance.MyCharacter.CharacterRecipeManager.RecipeList.ContainsValue(recipe)) {
                //Debug.Log(MyDisplayName + ".RecipeItem.Use(): Player does not have the recipe: " + recipe.MyDisplayName);
                bool returnValue = base.Use();
                if (returnValue == false) {
                    return false;
                }
                // learn recipe if the character has the right skill
                if (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.AbilityList.ContainsValue(recipe.CraftAbility)) {
                    PlayerManager.MyInstance.MyCharacter.CharacterRecipeManager.LearnRecipe(recipe);
                    MessageFeedManager.MyInstance.WriteMessage("You learned the recipe " + recipe.DisplayName);
                    Remove();
                } else {
                    MessageFeedManager.MyInstance.WriteMessage("To learn this recipe, you must know " + recipe.CraftAbility + "!");
                }
                return returnValue;
            } else {
                MessageFeedManager.MyInstance.WriteMessage("You already know this recipe!");
                return false;
            }
        }

        public override string GetSummary() {
            string returnString = base.GetSummary();
            if (recipe != null) {
                string alreadyKnownString = string.Empty;
                if (PlayerManager.MyInstance.MyCharacter.CharacterRecipeManager.RecipeList.ContainsValue(recipe)) {
                    alreadyKnownString = "<color=red>already known</color>\n";
                }
                string abilityKnownString = string.Empty;
                if (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.AbilityList.ContainsValue(recipe.CraftAbility)) {
                    abilityKnownString = "<color=white>Requires: " + recipe.CraftAbility.DisplayName  + "</color>\n";
                } else {
                    abilityKnownString = "<color=red>Requires: " + recipe.CraftAbility.DisplayName + "</color>\n";
                }
                returnString += string.Format("\n<color=green>Recipe</color>\n{0}{1}{2}", alreadyKnownString, abilityKnownString, recipe.MyOutput.GetSummary());
            }
            return returnString;
        }

        public override void SetupScriptableObjects() {
            //Debug.Log("RecipeItem.SetupScriptableObjects():");
            base.SetupScriptableObjects();

            if (recipeName != null && recipeName != string.Empty) {
                Recipe tmpRecipe = SystemRecipeManager.MyInstance.GetResource(recipeName);
                if (tmpRecipe != null) {
                    recipe = tmpRecipe;
                } else {
                    Debug.LogError("RecipeItem.SetupScriptableObjects(): Could not find recipe : " + recipeName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

        }
    }

}