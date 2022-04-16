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
        [ResourceSelector(resourceType = typeof(Recipe))]
        private string recipeName = string.Empty;

        private Recipe recipe = null;

        public override bool Use() {
            //Debug.Log(MyDisplayName + ".RecipeItem.Use()");
            if (!playerManager.MyCharacter.CharacterRecipeManager.RecipeList.ContainsValue(recipe)) {
                //Debug.Log(MyDisplayName + ".RecipeItem.Use(): Player does not have the recipe: " + recipe.MyDisplayName);
                bool returnValue = base.Use();
                if (returnValue == false) {
                    return false;
                }
                // learn recipe if the character has the right skill
                if (playerManager.MyCharacter.CharacterAbilityManager.AbilityList.ContainsValue(recipe.CraftAbility)) {
                    playerManager.MyCharacter.CharacterRecipeManager.LearnRecipe(recipe);
                    messageFeedManager.WriteMessage("You learned the recipe " + recipe.DisplayName);
                    Remove();
                } else {
                    messageFeedManager.WriteMessage("To learn this recipe, you must know " + recipe.CraftAbility.DisplayName + "!");
                }
                return returnValue;
            } else {
                messageFeedManager.WriteMessage("You already know this recipe!");
                return false;
            }
        }

        public override string GetDescription(ItemQuality usedItemQuality) {
            string returnString = base.GetDescription(usedItemQuality);
            if (recipe != null) {
                string alreadyKnownString = string.Empty;
                if (playerManager.MyCharacter.CharacterRecipeManager.RecipeList.ContainsValue(recipe)) {
                    alreadyKnownString = "<color=red>already known</color>\n";
                }
                string abilityKnownString = string.Empty;
                if (playerManager.MyCharacter.CharacterAbilityManager.AbilityList.ContainsValue(recipe.CraftAbility)) {
                    abilityKnownString = "<color=white>Requires: " + recipe.CraftAbility.DisplayName  + "</color>\n";
                } else {
                    abilityKnownString = "<color=red>Requires: " + recipe.CraftAbility.DisplayName + "</color>\n";
                }
                returnString += string.Format("\n<color=green>Recipe</color>\n{0}{1}{2}", alreadyKnownString, abilityKnownString, recipe.Output.GetDescription());
            }
            return returnString;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            //Debug.Log("RecipeItem.SetupScriptableObjects():");
            base.SetupScriptableObjects(systemGameManager);

            if (recipeName != null && recipeName != string.Empty) {
                Recipe tmpRecipe = systemDataFactory.GetResource<Recipe>(recipeName);
                if (tmpRecipe != null) {
                    recipe = tmpRecipe;
                } else {
                    Debug.LogError("RecipeItem.SetupScriptableObjects(): Could not find recipe : " + recipeName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

        }
    }

}