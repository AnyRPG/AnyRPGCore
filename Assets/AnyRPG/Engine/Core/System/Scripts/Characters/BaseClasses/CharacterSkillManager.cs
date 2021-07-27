using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterSkillManager {

        private BaseCharacter baseCharacter;

        private Dictionary<string, Skill> skillList = new Dictionary<string, Skill>();

        public Dictionary<string, Skill> MySkillList { get => skillList; }

        //public List<string> MySkillList { get => skillList;}
        public CharacterSkillManager(BaseCharacter baseCharacter) {
            this.baseCharacter = baseCharacter;
        }

        public void Init() {
            UpdateSkillList(baseCharacter.CharacterStats.Level);
        }

        public void UpdateSkillList(int newLevel) {
            //Debug.Log("CharacterSkillManager.UpdateSkillList()");
            foreach (Skill skill in SystemSkillManager.Instance.GetResourceList()) {
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
                skillList[SystemResourceManager.prepareStringForMatch(newSkill.DisplayName)] = newSkill;
                foreach (BaseAbility ability in newSkill.MyAbilityList) {
                    baseCharacter.CharacterAbilityManager.LearnAbility(ability);
                }
                foreach (Recipe recipe in SystemRecipeManager.Instance.GetResourceList()) {
                    if (baseCharacter.CharacterStats.Level >= recipe.RequiredLevel && recipe.AutoLearn == true && newSkill.MyAbilityList.Contains(recipe.CraftAbility)) {
                        PlayerManager.Instance.MyCharacter.CharacterRecipeManager.LearnRecipe(recipe);
                    }
                }

                SystemEventManager.Instance.NotifyOnSkillListChanged(newSkill);
            }
        }

        public void LoadSkill(string skillName) {
            //Debug.Log("CharacterSkillManager.LoadSkill()");
            string keyName = SystemResourceManager.prepareStringForMatch(skillName);
            if (!skillList.ContainsKey(keyName)) {
                skillList[keyName] = SystemSkillManager.Instance.GetResource(skillName);
            }
        }


        public void UnlearnSkill(Skill oldSkill) {
            if (skillList.ContainsValue(oldSkill)) {
                skillList.Remove(SystemResourceManager.prepareStringForMatch(oldSkill.DisplayName));
                foreach (BaseAbility ability in oldSkill.MyAbilityList) {
                    baseCharacter.CharacterAbilityManager.UnlearnAbility(ability);
                }
            }
        }


    }

}