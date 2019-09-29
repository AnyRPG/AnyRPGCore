﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillButton : TransparencyButton {

    [SerializeField]
    private string skillName;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private Text skillNameText;

    [SerializeField]
    private Text description;

    public void AddSkill(string skillName) {
        //Debug.Log("SkillButton.AddSkill(" + (skillName != null && skillName != string.Empty ? skillName : "null") + ")");
        this.skillName = skillName;
        Skill skill = SystemSkillManager.MyInstance.GetResource(skillName);
        if (skill != null) {
            icon.sprite = skill.MyIcon;
            icon.color = Color.white;
            skillNameText.text = skill.MyName;
            description.text = skill.GetSummary();
        } else {
            Debug.Log("SkillButton.AddSkill(): failed to get skill!!!");
        }
    }

    public void ClearSkill() {
        icon.sprite = null;
        icon.color = new Color32(0, 0, 0, 0);
        skillNameText.text = string.Empty;
        description.text = string.Empty;
    }

    /*
    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("AbilityButton.OnPointerClick()");
        if (eventData.button == PointerEventData.InputButton.Left) {
            Debug.Log("AbilityButton.OnPointerClick(): left click");
            HandScript.MyInstance.TakeMoveable(ability);
        }
        if (eventData.button == PointerEventData.InputButton.Right) {
            Debug.Log("AbilityButton.OnPointerClick(): right click");
            PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
        }
    }
    */
}
