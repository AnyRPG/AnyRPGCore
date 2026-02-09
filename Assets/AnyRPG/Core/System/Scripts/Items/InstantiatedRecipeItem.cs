using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class InstantiatedRecipeItem : InstantiatedItem {

        private RecipeItem recipeItem = null;

        public InstantiatedRecipeItem(SystemGameManager systemGameManager, long instanceId, RecipeItem recipeItem, ItemQuality itemQuality) : base(systemGameManager, instanceId, recipeItem, itemQuality) {
            this.recipeItem = recipeItem;
        }

        public override bool Use(UnitController sourceUnitController) {
            //Debug.Log(MyDisplayName + ".RecipeItem.Use()");
            if (!sourceUnitController.CharacterRecipeManager.RecipeList.ContainsValue(recipeItem.Recipe)) {
                //Debug.Log(MyDisplayName + ".RecipeItem.Use(): Player does not have the recipe: " + recipe.MyDisplayName);
                bool returnValue = base.Use(sourceUnitController);
                if (returnValue == false) {
                    return false;
                }
                // learn recipe if the character has the right skill
                if (sourceUnitController.CharacterAbilityManager.AbilityList.ContainsValue(recipeItem.Recipe.CraftAbility)) {
                    sourceUnitController.CharacterRecipeManager.LearnRecipe(recipeItem.Recipe);
                    sourceUnitController.WriteMessageFeedMessage("You learned the recipe " + recipeItem.Recipe.DisplayName);
                    Remove();
                } else {
                    sourceUnitController.WriteMessageFeedMessage("To learn this recipe, you must know " + recipeItem.Recipe.CraftAbility.DisplayName + "!");
                }
                return returnValue;
            } else {
                sourceUnitController.WriteMessageFeedMessage("You already know this recipe!");
                return false;
            }
        }


        public override string GetDescription() {
            //Debug.Log($"{item.ResourceName}.InstantiatedCurrencyItem.GetDescription()");

            return base.GetDescription() + recipeItem.GetRecipeItemDescription();
        }

    }

}