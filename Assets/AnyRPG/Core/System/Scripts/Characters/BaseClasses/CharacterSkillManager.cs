using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterSkillManager : ConfiguredClass {

        UnitController unitController;

        private Dictionary<string, Skill> skillList = new Dictionary<string, Skill>();

        // game manager references
        protected PlayerManager playerManager = null;
        protected SystemEventManager systemEventManager = null;

        public Dictionary<string, Skill> MySkillList { get => skillList; }

        //public List<string> MySkillList { get => skillList;}
        public CharacterSkillManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void UpdateSkillList(int newLevel) {
            //Debug.Log("CharacterSkillManager.UpdateSkillList()");
            foreach (Skill skill in systemDataFactory.GetResourceList<Skill>()) {
                if (!HasSkill(skill) && skill.RequiredLevel <= newLevel && skill.AutoLearn == true) {
                    LearnSkill(skill);
                }
            }
        }

        public bool HasSkill(Skill checkSkill) {
            //Debug.Log($"{gameObject.name}.CharacterSkillManager.HasSkill(" + skillName + ")");
            if (skillList.ContainsValue(checkSkill)) {
                return true;
            }
            return false;
        }

        public void LearnSkill(Skill newSkill) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSkillManager.LearnSkill({newSkill.ResourceName})");

            if (!skillList.ContainsValue(newSkill)) {
                skillList[newSkill.ResourceName] = newSkill;
                foreach (AbilityProperties ability in newSkill.AbilityList) {
                    unitController.CharacterAbilityManager.LearnAbility(ability);
                }
                foreach (Recipe recipe in systemDataFactory.GetResourceList<Recipe>()) {
                    if (unitController.CharacterStats.Level >= recipe.RequiredLevel && recipe.AutoLearn == true && newSkill.AbilityList.Contains(recipe.CraftAbility)) {
                        unitController.CharacterRecipeManager.LearnRecipe(recipe);
                    }
                }

                unitController.UnitEventController.NotifyOnLearnSkill(newSkill);
            }
        }

        public void LoadSkill(string skillName) {
            //Debug.Log("CharacterSkillManager.LoadSkill()");
            
            // don't crash on loading old save Data
            if (skillName == null || skillName == string.Empty) {
                return;
            }
            if (!skillList.ContainsKey(skillName)) {
                skillList[skillName] = systemDataFactory.GetResource<Skill>(skillName);
            }
        }


        public void UnLearnSkill(Skill oldSkill) {
            if (skillList.ContainsValue(oldSkill)) {
                skillList.Remove(oldSkill.ResourceName);
                foreach (AbilityProperties ability in oldSkill.AbilityList) {
                    unitController.CharacterAbilityManager.UnlearnAbility(ability);
                }
            }
            unitController.UnitEventController.NotifyOnUnLearnSkill(oldSkill);

        }


    }

}