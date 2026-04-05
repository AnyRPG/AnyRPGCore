using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterSkillManager : ConfiguredClass {

        UnitController unitController;

        private Dictionary<string, CharacterSkillData> skillList = new Dictionary<string, CharacterSkillData>();

        // game manager references
        protected PlayerManagerClient playerManagerClient = null;
        protected SystemEventManager systemEventManager = null;

        public Dictionary<string, CharacterSkillData> SkillList { get => skillList; }

        public CharacterSkillManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
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
            if (skillList.ContainsKey(checkSkill.ResourceName)) {
                return true;
            }
            return false;
        }

        public void LearnSkill(Skill newSkill) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSkillManager.LearnSkill({newSkill.ResourceName})");

            if (!skillList.ContainsKey(newSkill.ResourceName)) {
                skillList[newSkill.ResourceName] = new CharacterSkillData {
                    Skill = newSkill,
                    SkillLevel = 1
                };
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

        public void LoadSkill(CharacterSkillSaveData characterSkillSaveData) {
            //Debug.Log("CharacterSkillManager.LoadSkill()");
            
            // don't crash on loading old save Data
            if (characterSkillSaveData?.SkillResourceName == null || characterSkillSaveData.SkillResourceName == string.Empty) {
                return;
            }
            if (!skillList.ContainsKey(characterSkillSaveData.SkillResourceName)) {
                Skill skill = systemDataFactory.GetResource<Skill>(characterSkillSaveData.SkillResourceName);
                CharacterSkillData characterSkillData = new CharacterSkillData {
                    Skill = skill,
                    SkillLevel = characterSkillSaveData.SkillLevel
                };
                skillList[characterSkillSaveData.SkillResourceName] = characterSkillData;
            }
        }


        public void UnLearnSkill(Skill oldSkill) {
            if (skillList.ContainsKey(oldSkill.ResourceName)) {
                skillList.Remove(oldSkill.ResourceName);
                foreach (AbilityProperties ability in oldSkill.AbilityList) {
                    unitController.CharacterAbilityManager.UnlearnAbility(ability);
                }
            }
            unitController.UnitEventController.NotifyOnUnLearnSkill(oldSkill);

        }

        public int GetSkillLevel(Skill skill) {
            if (skillList.ContainsKey(skill.ResourceName)) {
                return skillList[skill.ResourceName].SkillLevel;
            }
            return 0;
        }

        public void AddSkillLevel(Skill skill, int addLevel) {
            if (skillList.ContainsKey(skill.ResourceName)) {
                CharacterSkillData characterSkillData = skillList[skill.ResourceName];
                if (characterSkillData.SkillLevel < skill.GetSkillCapForLevel(playerManagerClient.UnitController.CharacterStats.Level)) {
                    characterSkillData.SkillLevel += addLevel;
                    if (characterSkillData.SkillLevel > skill.GetSkillCapForLevel(playerManagerClient.UnitController.CharacterStats.Level)) {
                        characterSkillData.SkillLevel = skill.GetSkillCapForLevel(playerManagerClient.UnitController.CharacterStats.Level);
                    }
                    unitController.UnitEventController.NotifyOnAddSkillLevel(skill, addLevel);
                }
            }
        }
    }

}