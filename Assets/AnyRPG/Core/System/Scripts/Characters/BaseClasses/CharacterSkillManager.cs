using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterSkillManager : ConfiguredClass {

        private BaseCharacter baseCharacter;

        private Dictionary<string, Skill> skillList = new Dictionary<string, Skill>();

        // game manager references
        protected PlayerManager playerManager = null;
        protected SystemEventManager systemEventManager = null;

        public Dictionary<string, Skill> MySkillList { get => skillList; }

        //public List<string> MySkillList { get => skillList;}
        public CharacterSkillManager(BaseCharacter baseCharacter, SystemGameManager systemGameManager) {
            this.baseCharacter = baseCharacter;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void Init() {
            UpdateSkillList(baseCharacter.CharacterStats.Level);
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
            //Debug.Log(gameObject.name + ".CharacterSkillManager.HasSkill(" + skillName + ")");
            if (skillList.ContainsValue(checkSkill)) {
                return true;
            }
            return false;
        }

        public void LearnSkill(Skill newSkill) {
            //Debug.Log("CharacterSkillManager.LearnSkill(" + skill.name + ")");
            if (!skillList.ContainsValue(newSkill)) {
                skillList[SystemDataFactory.PrepareStringForMatch(newSkill.DisplayName)] = newSkill;
                foreach (BaseAbility ability in newSkill.MyAbilityList) {
                    baseCharacter.CharacterAbilityManager.LearnAbility(ability);
                }
                foreach (Recipe recipe in systemDataFactory.GetResourceList<Recipe>()) {
                    if (baseCharacter.CharacterStats.Level >= recipe.RequiredLevel && recipe.AutoLearn == true && newSkill.MyAbilityList.Contains(recipe.CraftAbility)) {
                        playerManager.MyCharacter.CharacterRecipeManager.LearnRecipe(recipe);
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
            string keyName = SystemDataFactory.PrepareStringForMatch(skillName);
            if (!skillList.ContainsKey(keyName)) {
                skillList[keyName] = systemDataFactory.GetResource<Skill>(skillName);
            }
        }


        public void UnlearnSkill(Skill oldSkill) {
            if (skillList.ContainsValue(oldSkill)) {
                skillList.Remove(SystemDataFactory.PrepareStringForMatch(oldSkill.DisplayName));
                foreach (BaseAbility ability in oldSkill.MyAbilityList) {
                    baseCharacter.CharacterAbilityManager.UnlearnAbility(ability);
                }
            }
        }


    }

}