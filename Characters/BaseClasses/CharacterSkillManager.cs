using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkillManager : MonoBehaviour, ICharacterSkillManager {

    protected ICharacter baseCharacter;

    protected Dictionary<string, Skill> skillList = new Dictionary<string, Skill>();

    public ICharacter MyBaseCharacter {
        get => baseCharacter;
        set => baseCharacter = value;
    }

    public Dictionary<string, Skill> MySkillList { get => skillList; }

    //public List<string> MySkillList { get => skillList;}

    protected virtual void Awake() {
        //Debug.Log("CharacterAbilityManager.Awake()");
        baseCharacter = GetComponent<BaseCharacter>() as ICharacter;
    }

    protected virtual void Start() {
        //Debug.Log("CharacterAbilityManager.Start()");
        CreateEventReferences();
        UpdateSkillList(baseCharacter.MyCharacterStats.MyLevel);
    }

    public virtual void OnDisable() {
        CleanupEventReferences();
    }

    public void CreateEventReferences() {
        SystemEventManager.MyInstance.OnLevelChanged += UpdateSkillList;
    }

    public void CleanupEventReferences() {
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
        //Debug.Log("CharacterSkillManager.HasSkill(" + skillName + ")");
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
