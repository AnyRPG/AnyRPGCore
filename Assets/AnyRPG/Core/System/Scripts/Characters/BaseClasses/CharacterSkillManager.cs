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
            //Debug.Log("CharacterSkillManager.LearnSkill(" + newSkill.DisplayName + ")");

            if (!skillList.ContainsValue(newSkill)) {
                skillList[SystemDataUtility.PrepareStringForMatch(newSkill.ResourceName)] = newSkill;
                foreach (BaseAbilityProperties ability in newSkill.AbilityList) {
                    unitController.CharacterAbilityManager.LearnAbility(ability);
                }
                foreach (Recipe recipe in systemDataFactory.GetResourceList<Recipe>()) {
                    if (unitController.CharacterStats.Level >= recipe.RequiredLevel && recipe.AutoLearn == true && newSkill.AbilityList.Contains(recipe.CraftAbility)) {
                        playerManager.UnitController.CharacterRecipeManager.LearnRecipe(recipe);
                    }
                }

                systemEventManager.NotifyOnSkillListChanged(newSkill);
            }
        }

        public void LoadSkill(string skillName) {
            //Debug.Log("CharacterSkillManager.LoadSkill()");
            
            // don't crash on loading old save Data
            if (skillName == null || skillName == string.Empty) {
                return;
            }
            string keyName = SystemDataUtility.PrepareStringForMatch(skillName);
            if (!skillList.ContainsKey(keyName)) {
                skillList[keyName] = systemDataFactory.GetResource<Skill>(skillName);
            }
        }


        public void UnlearnSkill(Skill oldSkill) {
            if (skillList.ContainsValue(oldSkill)) {
                skillList.Remove(SystemDataUtility.PrepareStringForMatch(oldSkill.ResourceName));
                foreach (BaseAbilityProperties ability in oldSkill.AbilityList) {
                    unitController.CharacterAbilityManager.UnlearnAbility(ability);
                }
            }
        }


    }

}