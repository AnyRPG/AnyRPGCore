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

        public Skill MySkill { get => skill; }

        public void SetSkill(Skill newSkill) {
            if (newSkill != null) {
                this.skill = newSkill;
            }
        }

        public override void Select() {
            //Debug.Log(gameObject.name + ".SkillTrainerSkillScript.Select()");

            base.Select();
            SkillTrainerUI.MyInstance.MySelectedSkillTrainerSkillScript = this;

            SkillTrainerUI.MyInstance.ShowDescription(skill);

        }

        public override void DeSelect() {
            base.DeSelect();
        }

    }

}