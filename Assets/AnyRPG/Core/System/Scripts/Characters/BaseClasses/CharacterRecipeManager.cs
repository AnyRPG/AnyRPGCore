using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterRecipeManager : ConfiguredClass {

        protected UnitController unitController;

        protected Dictionary<string, Recipe> recipeList = new Dictionary<string, Recipe>();

        public Dictionary<string, Recipe> RecipeList { get => recipeList; }

        public CharacterRecipeManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public virtual void UpdateRecipeList(int newLevel) {
            foreach (Recipe recipe in systemDataFactory.GetResourceList<Recipe>()) {
                foreach (CharacterSkillData characterSkillData in unitController.CharacterSkillManager.SkillList.Values) {
                    if (!HasRecipe(recipe) && recipe.RequiredLevel <= newLevel && recipe.AutoLearn == true && characterSkillData.Skill.AbilityList.Contains(recipe.CraftAbility)) {
                        LearnRecipe(recipe);
                    }
                }
            }
        }

        public bool HasRecipe(Recipe checkRecipe) {
            if (recipeList.ContainsValue(checkRecipe)) {
                return true;
            }
            return false;
        }

        public void LearnRecipe(Recipe newRecipe) {
            //Debug.Log("CharacterRecipeManager.LearnRecipe(" + newRecipe.name + ")");
            if (newRecipe == null) {
                return;
            }
            if (!recipeList.ContainsValue(newRecipe)) {
                recipeList[newRecipe.ResourceName] = newRecipe;
                unitController.UnitEventController.NotifyOnLearnRecipe(newRecipe);
            }
        }

        public void LoadRecipe(string recipeName) {
            //Debug.Log("CharacterRecipeManager.LoadRecipe(" + recipeName + ")");

            // don't crash when loading old save data
            if (recipeName == null || recipeName == string.Empty) {
                return;
            }
            if (!recipeList.ContainsKey(recipeName)) {
                recipeList[recipeName] = systemDataFactory.GetResource<Recipe>(recipeName);
                if (recipeList[recipeName] == null) {
                    // failed to get a valid recipe
                    recipeList.Remove(recipeName);
                }
            }
        }


        public void UnlearnRecipe(Recipe oldRecipe) {
            if (recipeList.ContainsValue(oldRecipe)) {
                recipeList.Remove(oldRecipe.ResourceName);
                unitController.UnitEventController.NotifyOnUnlearnRecipe(oldRecipe);

            }
        }


    }

}