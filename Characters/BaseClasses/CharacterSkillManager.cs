using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterSkillManager : MonoBehaviour {

        protected BaseCharacter baseCharacter;

        protected Dictionary<string, Skill> skillList = new Dictionary<string, Skill>();

        public BaseCharacter MyBaseCharacter {
            get => baseCharacter;
            set => baseCharacter = value;
        }

        public Dictionary<string, Skill> MySkillList { get => skillList; }

        //public List<string> MySkillList { get => skillList;}

        protected virtual void Awake() {
            //Debug.Log("CharacterAbilityManager.Awake()");
            baseCharacter = GetComponent<BaseCharacter>();
        }

        protected virtual void Start() {
            //Debug.Log("CharacterAbilityManager.Start()");
            CreateEventSubscriptions();
            UpdateSkillList(baseCharacter.CharacterStats.Level);
        }

        public virtual void OnDisable() {
            CleanupEventSubscriptions();
        }

        public void CreateEventSubscriptions() {
            SystemEventManager.MyInstance.OnLevelChanged += UpdateSkillList;
        }

        public void CleanupEventSubscriptions() {
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnLevelChanged -= UpdateSkillList;
            }
        }

        /*
        public List<string> GetSkillList() {

            return skillList.Keys;
        }
        */

        public void UpdateSkillList(int newLevel) {
            //Debug.Log("CharacterSkillManager.UpdateSkillList()");
            foreach (Skill skill in SystemSkillManager.MyInstance.GetResourceList()) {
                if (!HasSkill(skill) && skill.MyRequiredLevel <= newLevel && skill.MyAutoLearn == true) {
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
                skillList[SystemResourceManager.prepareStringForMatch(newSkill.MyName)] = newSkill;
                foreach (BaseAbility ability in newSkill.MyAbilityList) {
                    MyBaseCharacter.CharacterAbilityManager.LearnAbility(ability);
                }
                SystemEventManager.MyInstance.NotifyOnSkillListChanged(newSkill);
            }
        }

        public void LoadSkill(string skillName) {
            //Debug.Log("CharacterSkillManager.LoadSkill()");
            string keyName = SystemResourceManager.prepareStringForMatch(skillName);
            if (!skillList.ContainsKey(keyName)) {
                skillList[keyName] = SystemSkillManager.MyInstance.GetResource(skillName);
            }
        }


        public void UnlearnSkill(Skill oldSkill) {
            if (skillList.ContainsValue(oldSkill)) {
                skillList.Remove(SystemResourceManager.prepareStringForMatch(oldSkill.MyName));
                foreach (BaseAbility ability in oldSkill.MyAbilityList) {
                    MyBaseCharacter.CharacterAbilityManager.UnlearnAbility(ability);
                }
            }
        }


    }

}