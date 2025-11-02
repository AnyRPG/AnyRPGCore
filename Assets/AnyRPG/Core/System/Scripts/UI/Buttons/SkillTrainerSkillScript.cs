using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class SkillTrainerSkillScript : HighlightButton {

        protected Skill skill;
        protected int skillId = 0;

        protected string skillName;

        protected SkillTrainerPanel skillTrainerUI = null;

        public Skill Skill { get => skill; }
        public int SkillId { get => skillId; }

        public void SetSkill(SkillTrainerPanel skillTrainerUI, KeyValuePair<int, Skill> newSkillPair) {
            this.skillTrainerUI = skillTrainerUI;
            if (newSkillPair.Value != null) {
                this.skill = newSkillPair.Value;
            }
            this.skillId = newSkillPair.Key;
        }

        public override void Select() {
            //Debug.Log($"{gameObject.name}.SkillTrainerSkillScript.Select()");

            base.Select();
            skillTrainerUI.SetSelectedButton(this);
        }

    }

}