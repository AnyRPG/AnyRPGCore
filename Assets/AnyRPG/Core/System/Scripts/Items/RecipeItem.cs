using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "RecipeItem", menuName = "AnyRPG/Inventory/Items/RecipeItem", order = 1)]
    public class RecipeItem : Item {

        [Header("Recipe Item")]

        [Tooltip("The power resource to refill when this potion is used")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Recipe))]
        private string recipeName = string.Empty;

        private Recipe recipe = null;

        public Recipe Recipe { get => recipe; }

        public override InstantiatedItem GetNewInstantiatedItem(SystemGameManager systemGameManager, long itemInstanceId, Item item, ItemQuality usedItemQuality) {
            if ((item is RecipeItem) == false) {
                return null;
            }
            return new InstantiatedRecipeItem(systemGameManager, itemInstanceId, item as RecipeItem, usedItemQuality);
        }


        public override string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            //Debug.Log($"CurrencyItem.GetDescription({(usedItemQuality == null ? "null" : usedItemQuality.ResourceName)}, {usedItemLevel});");

            return base.GetDescription(usedItemQuality, usedItemLevel) + GetRecipeItemDescription();
        }

        public string GetRecipeItemDescription() {
            string returnString = string.Empty;
            if (recipe != null) {
                string alreadyKnownString = string.Empty;
                if (playerManagerClient.UnitController.CharacterRecipeManager.RecipeList.ContainsValue(recipe)) {
                    alreadyKnownString = "<color=red>already known</color>\n";
                }
                string abilityKnownString = string.Empty;
                if (playerManagerClient.UnitController.CharacterAbilityManager.AbilityList.ContainsValue(recipe.CraftAbility)) {
                    abilityKnownString = $"<color=white>Requires: {recipe.CraftAbility.DisplayName}</color>\n";
                } else {
                    abilityKnownString = $"<color=red>Requires: {recipe.CraftAbility.DisplayName}</color>\n";
                }
                // add string for required skill level if there is a required skill
                string skillLevelString = string.Empty;
                if (recipe.Skill != null) {
                    if (playerManagerClient.UnitController.CharacterSkillManager.HasSkill(recipe.Skill) && playerManagerClient.UnitController.CharacterSkillManager.GetSkillLevel(recipe.Skill) > recipe.RequiredSkillLevel) {
                        skillLevelString = $"<color=white>Requires: {recipe.Skill.DisplayName} level {recipe.RequiredLevel}</color>\n";
                    } else {
                        skillLevelString = $"<color=red>Requires: {recipe.Skill.DisplayName} level {recipe.RequiredLevel}</color>\n";
                    }
                }
                string characterLevelString = string.Empty;
                if (playerManagerClient.UnitController.CharacterStats.Level >= recipe.RequiredLevel) {
                    characterLevelString = $"<color=white>Requires Character Level {recipe.RequiredLevel}</color>\n";
                } else {
                    characterLevelString = $"<color=red>Requires Character Level {recipe.RequiredLevel}</color>\n";
                }
                returnString += $"\n<color=green>Recipe</color>\n{alreadyKnownString}{characterLevelString}{abilityKnownString}{skillLevelString}{recipe.Output.GetDescription()}";
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
                    Debug.LogError("RecipeItem.SetupScriptableObjects(): Could not find recipe : " + recipeName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }

        }
    }

}