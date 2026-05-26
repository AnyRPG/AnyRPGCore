using System.Collections.Generic;

namespace AnyRPG {
    public class CharacterSkillManager : ConfiguredClass {

        UnitController unitController;

        private Dictionary<string, CharacterSkillData> skillList = new Dictionary<string, CharacterSkillData>();

        // game manager references
        protected SystemEventManager systemEventManager = null;

        public Dictionary<string, CharacterSkillData> SkillList { get => skillList; }

        public CharacterSkillManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
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

                /*
                foreach (Recipe recipe in systemDataFactory.GetResourceList<Recipe>()) {
                    if (unitController.CharacterStats.Level >= recipe.RequiredLevel && recipe.AutoLearn == true && newSkill.AbilityList.Contains(recipe.CraftAbility)) {
                        unitController.CharacterRecipeManager.LearnRecipe(recipe);
                    }
                }
                */
                // use this instead since it has the calculation that includes the skill level and not just the character level
                unitController.CharacterRecipeManager.UpdateRecipeList(unitController.CharacterStats.Level);

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
                    SkillLevel = characterSkillSaveData.SkillLevel,
                    SkillExperience = characterSkillSaveData.SkillExperience
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
            AddSkillLevel(skill, addLevel, true);
        }

        public void AddSkillLevel(Skill skill, int addLevel, bool notify) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSkillManager.AddSkillLevel({skill.ResourceName}, {addLevel})");

            if (skillList.ContainsKey(skill.ResourceName) == false) {
                return;
            }

            CharacterSkillData characterSkillData = skillList[skill.ResourceName];
            if (characterSkillData.SkillLevel >= skill.GetSkillCapForLevel(unitController.CharacterStats.Level)) {
                return;
            }
            characterSkillData.SkillLevel += addLevel;
            if (characterSkillData.SkillLevel > skill.GetSkillCapForLevel(unitController.CharacterStats.Level)) {
                characterSkillData.SkillLevel = skill.GetSkillCapForLevel(unitController.CharacterStats.Level);
            }
            if (notify == true) {
                unitController.UnitEventController.NotifyOnAddSkillLevel(skill, addLevel);
            }
        }

        public void AddSkillExperience(Skill skill, int addExperience) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSkillManager.AddSkillExperience({skill.ResourceName}, {addExperience})");

            if (skillList.ContainsKey(skill.ResourceName) == false) {
                return;
            }
            CharacterSkillData characterSkillData = skillList[skill.ResourceName];
            if (skill.UseSkillExperience == true) {
                characterSkillData.SkillExperience += addExperience;
                while (characterSkillData.SkillLevel < skill.GetSkillCapForLevel(unitController.CharacterStats.Level) && characterSkillData.SkillExperience >= skill.SkillExperienceChart[characterSkillData.SkillLevel - 1]) {
                    characterSkillData.SkillExperience -= skill.SkillExperienceChart[characterSkillData.SkillLevel - 1];
                    // only notify on level up if the server is not active.
                    // This will prevent the server sending an extra message to the client to update the skill level,
                    // since the client will calculate the level up on its own when the experience is added.
                    AddSkillLevel(skill, 1, networkManagerServer.ServerModeActive == false);
                }
            }
            unitController.UnitEventController.NotifyOnAddSkillExperience(skill, addExperience);
        }

        public CharacterSkillData GetCharacterSkillData(Skill skill) {
            if (skillList.ContainsKey(skill.ResourceName)) {
                return skillList[skill.ResourceName];
            }
            return null;
        }

        /*
        public void SetSkillExperience(Skill skill, int experienceValue) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSkillManager.SetSkillExperience({skill.ResourceName}, {experienceValue})");

            if (skillList.ContainsKey(skill.ResourceName) == false) {
                return;
            }
            CharacterSkillData characterSkillData = skillList[skill.ResourceName];
            characterSkillData.SkillExperience = experienceValue;
        }
        */
    }
}