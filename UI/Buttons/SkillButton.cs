using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class SkillButton : TransparencyButton {

        [SerializeField]
        private string skillName;

        private Skill skill;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text skillNameText;

        [SerializeField]
        private Text description;

        public void AddSkill(Skill newSkill) {
            //Debug.Log("SkillButton.AddSkill(" + (skillName != null && skillName != string.Empty ? skillName : "null") + ")");
            skill = newSkill;
            if (skill != null) {
                icon.sprite = skill.MyIcon;
                icon.color = Color.white;
                skillNameText.text = skill.MyName;
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