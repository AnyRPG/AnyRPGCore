using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class SkillButton : TransparencyButton {

        [SerializeField]
        protected string skillName = string.Empty;

        protected Skill skill = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI skillNameText = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        public void AddSkill(Skill newSkill) {
            //Debug.Log("SkillButton.AddSkill(" + (skillName != null && skillName != string.Empty ? skillName : "null") + ")");
            skill = newSkill;
            if (skill != null) {
                icon.sprite = skill.Icon;
                icon.color = Color.white;
                skillNameText.text = skill.DisplayName;
                description.text = skill.GetSummary();
            } else {
                //Debug.Log("SkillButton.AddSkill(): failed to get skill!!!");
            }
        }

        public void ClearSkill() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            skillNameText.text = string.Empty;
            description.text = string.Empty;
        }

    }

}