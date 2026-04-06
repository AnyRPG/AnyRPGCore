using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class RecipeScript : HighlightButton {

        protected Recipe recipe;

        // game manager references
        protected CraftingManager craftingManager = null;
        protected PlayerManagerClient playerManagerClient = null;

        public Recipe Recipe { get => recipe; set => recipe = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            craftingManager = systemGameManager.CraftingManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        public void SetRecipe(Recipe newRecipe) {
            recipe = newRecipe;
            UpdateMaxCraftAmount();
        }

        public void UpdateMaxCraftAmount() {
            int maxCraftAmount = playerManagerClient.UnitController.CharacterCraftingManager.GetMaxCraftAmount(recipe);
            string craftString = string.Empty;
            if (maxCraftAmount > 0) {
                craftString = $" ({maxCraftAmount})";
            }
            string colorstring = GetColorString();
            // determine if recipe will give experience and make color green if it does
            text.text = $"<color={colorstring}>{recipe.Output.DisplayName}{craftString}</color>";
        }

        public string GetColorString() {
            if (recipe.Skill.UseSkillExperience == false && recipe.Skill.GiveCharacterExperience == false) {
                // default to white if recipe doesn't give any experience
                return "#ffffff";
            }
            if (recipe.Skill != null
                && recipe.Skill.UseSkillExperience
                && (recipe.MaxSkillExperienceLevel >= playerManagerClient.UnitController.CharacterSkillManager.GetSkillLevel(recipe.Skill) || recipe.MaxSkillExperienceLevel == 0)) {
                return "#00ff00";
            }
            if (recipe.Skill != null
                && recipe.Skill.GiveCharacterExperience
                && (recipe.MaxCharacterExperienceLevel >= playerManagerClient.UnitController.CharacterStats.Level || recipe.MaxCharacterExperienceLevel == 0)) {
                return "#00ff00";
            }

            // if recipe gives experience but character is at max skill level, gray it out
            return "#cccccc";
        }

        public override void Select() {
            //Debug.Log($"{gameObject.name}.RecipeScript.Select(): " + (recipe == null ? "null" : recipe.DisplayName));

            base.Select();
            craftingManager.SetSelectedRecipe(recipe);
        }


    }

}