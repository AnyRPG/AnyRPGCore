using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class SkillTrainerSkillScript : HighlightButton {

        private Skill skill;

        private string skillName;

        private SkillTrainerUI skillTrainerUI = null;

        public Skill MySkill { get => skill; }

        public void SetSkill(SkillTrainerUI skillTrainerUI, Skill newSkill) {
            this.skillTrainerUI = skillTrainerUI;
            if (newSkill != null) {
                this.skill = newSkill;
            }
        }

        public override void Select() {
            //Debug.Log(gameObject.name + ".SkillTrainerSkillScript.Select()");

            base.Select();
            skillTrainerUI.MySelectedSkillTrainerSkillScript = this;

            skillTrainerUI.ShowDescription(skill);

        }

        public override void DeSelect() {
            base.DeSelect();
        }

    }

}