using AnyRPG;
﻿using System.Collections;
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
        UpdateSkillList(baseCharacter.MyCharacterStats.MyLevel);
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
            if (!HasSkill(skill.MyName) && skill.MyRequiredLevel <= newLevel && skill.MyAutoLearn == true) {
                LearnSkill(skill.MyName);
            }
        }
    }

    public bool HasSkill(string skillName) {
        //Debug.Log(gameObject.name + ".CharacterSkillManager.HasSkill(" + skillName + ")");
        string keyName = SystemResourceManager.prepareStringForMatch(skillName);
        if (skillList.ContainsKey(keyName)) {
            return true;
        }
        return false;
    }

    public void LearnSkill(string skillName) {
        Skill skill = SystemSkillManager.MyInstance.GetResource(skillName);
        //Debug.Log("CharacterSkillManager.LearnSkill(" + skill.name + ")");
        string keyName = SystemResourceManager.prepareStringForMatch(skillName);
        if (!skillList.ContainsKey(keyName)) {
            skillList[keyName] = skill;
            foreach (BaseAbility ability in skill.MyAbilityList) {
                MyBaseCharacter.MyCharacterAbilityManager.LearnAbility(ability.MyName);
            }
            SystemEventManager.MyInstance.NotifyOnSkillListChanged(skill);
        }
    }

    public void LoadSkill(string skillName) {
        //Debug.Log("CharacterSkillManager.LoadSkill()");
        string keyName = SystemResourceManager.prepareStringForMatch(skillName);
        if (!skillList.ContainsKey(skillName)) {
            skillList[skillName] = SystemSkillManager.MyInstance.GetResource(skillName);
        }
    }


    public void UnlearnSkill(string skillName) {
        Skill skill = SystemSkillManager.MyInstance.GetResource(skillName);
        string keyName = SystemResourceManager.prepareStringForMatch(skillName);
        if (skillList.ContainsKey(keyName)) {
            skillList.Remove(skillName);
            foreach (BaseAbility ability in skill.MyAbilityList) {
                MyBaseCharacter.MyCharacterAbilityManager.UnlearnAbility(ability.MyName);
            }
        }
    }


}

}