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
                foreach (Skill skill in unitController.CharacterSkillManager.MySkillList.Values) {
                    if (!HasRecipe(recipe) && recipe.RequiredLevel <= newLevel && recipe.AutoLearn == true && skill.AbilityList.Contains(recipe.CraftAbility)) {
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
                recipeList[SystemDataUtility.PrepareStringForMatch(newRecipe.ResourceName)] = newRecipe;
                EventParamProperties eventParamProperties = new EventParamProperties();
                eventParamProperties.simpleParams.StringParam = newRecipe.ResourceName;
                SystemEventManager.TriggerEvent("OnRecipeListChanged", eventParamProperties);
            }
        }

        public void LoadRecipe(string recipeName) {
            //Debug.Log("CharacterRecipeManager.LoadRecipe(" + recipeName + ")");

            // don't crash when loading old save data
            if (recipeName == null || recipeName == string.Empty) {
                return;
            }
            string keyName = SystemDataUtility.PrepareStringForMatch(recipeName);
            if (!recipeList.ContainsKey(keyName)) {
                recipeList[keyName] = systemDataFactory.GetResource<Recipe>(recipeName);
                if (recipeList[keyName] == null) {
                    // failed to get a valid recipe
                    recipeList.Remove(keyName);
                }
            }
        }


        public void UnlearnRecipe(Recipe oldRecipe) {
            if (recipeList.ContainsValue(oldRecipe)) {
                recipeList.Remove(SystemDataUtility.PrepareStringForMatch(oldRecipe.ResourceName));
            }
        }


    }

}