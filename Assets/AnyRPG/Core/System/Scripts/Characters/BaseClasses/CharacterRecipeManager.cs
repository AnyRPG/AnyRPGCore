using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterRecipeManager : ConfiguredClass {

        protected BaseCharacter baseCharacter;

        protected Dictionary<string, Recipe> recipeList = new Dictionary<string, Recipe>();

        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public BaseCharacter BaseCharacter {
            get => baseCharacter;
            set => baseCharacter = value;
        }

        public Dictionary<string, Recipe> RecipeList { get => recipeList; }

        public CharacterRecipeManager(BaseCharacter baseCharacter, SystemGameManager systemGameManager) {
            this.baseCharacter = baseCharacter;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void Init() {
            UpdateRecipeList(baseCharacter.CharacterStats.Level);
        }

        public virtual void UpdateRecipeList(int newLevel) {
            foreach (Recipe recipe in systemDataFactory.GetResourceList<Recipe>()) {
                foreach (Skill skill in baseCharacter.CharacterSkillManager.MySkillList.Values) {
                    if (!HasRecipe(recipe) && recipe.RequiredLevel <= newLevel && recipe.AutoLearn == true && skill.MyAbilityList.Contains(recipe.CraftAbility)) {
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
                recipeList[SystemDataFactory.PrepareStringForMatch(newRecipe.DisplayName)] = newRecipe;
                EventParamProperties eventParamProperties = new EventParamProperties();
                eventParamProperties.simpleParams.StringParam = newRecipe.DisplayName;
                SystemEventManager.TriggerEvent("OnRecipeListChanged", eventParamProperties);
            }
        }

        public void LoadRecipe(string recipeName) {
            //Debug.Log("CharacterRecipeManager.LoadRecipe(" + recipeName + ")");

            // don't crash when loading old save data
            if (recipeName == null || recipeName == string.Empty) {
                return;
            }
            string keyName = SystemDataFactory.PrepareStringForMatch(recipeName);
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
                recipeList.Remove(SystemDataFactory.PrepareStringForMatch(oldRecipe.DisplayName));
            }
        }


    }

}