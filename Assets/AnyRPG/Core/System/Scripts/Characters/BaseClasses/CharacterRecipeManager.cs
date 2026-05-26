using System.Collections.Generic;

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
                if (CanAutoLearnRecipe(recipe, newLevel)) {
                    LearnRecipe(recipe);
                }
            }
        }

        private bool CanAutoLearnRecipe(Recipe recipe, int newLevel) {
            // recipe cannot be auto learned
            if (recipe.AutoLearn == false) {
                return false;
            }
            // recipe is already known
            if (HasRecipe(recipe)) {
                return false;
            }
            // recipe is above the character's level
            if (recipe.RequiredLevel > newLevel) {
                return false;
            }
            if (recipe.Skill != null) {
                if (unitController.CharacterSkillManager.HasSkill(recipe.Skill) == false) {
                    return false;
                }
                CharacterSkillData skillData = unitController.CharacterSkillManager.GetCharacterSkillData(recipe.Skill);
                if (skillData.SkillLevel < recipe.RequiredSkillLevel) {
                    return false;
                }
            }
            return true;
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