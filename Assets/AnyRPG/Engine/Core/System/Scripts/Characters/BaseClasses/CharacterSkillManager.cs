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
            foreach (Skill skill in SystemDataFactory.Instance.GetResourceList<Skill>()) {
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
                foreach (Recipe recipe in SystemDataFactory.Instance.GetResourceList<Recipe>()) {
                    if (baseCharacter.CharacterStats.Level >= recipe.RequiredLevel && recipe.AutoLearn == true && newSkill.MyAbilityList.Contains(recipe.CraftAbility)) {
                        SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterRecipeManager.LearnRecipe(recipe);
                    }
                }

                SystemGameManager.Instance.SystemEventManager.NotifyOnSkillListChanged(newSkill);
            }
        }

        public void LoadSkill(string skillName) {
            //Debug.Log("CharacterSkillManager.LoadSkill()");
            string keyName = SystemDataFactory.PrepareStringForMatch(skillName);
            if (!skillList.ContainsKey(keyName)) {
                skillList[keyName] = SystemDataFactory.Instance.GetResource<Skill>(skillName);
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