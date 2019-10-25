using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
// this is almost identical to questscript

public class SkillTrainerSkillScript : HighlightButton {

    private string skillName;

    public string MySkillName { get => skillName; }

    public void SetSkillName(string skillName) {
        if (skillName != null && skillName != string.Empty) {
            this.skillName = skillName;
        }
    }

    public override void Select() {
        //Debug.Log(gameObject.name + ".SkillTrainerSkillScript.Select()");

        base.Select();
        SkillTrainerUI.MyInstance.MySelectedSkillTrainerSkillScript = this;

        //GetComponent<Text>().color = Color.red;
        SkillTrainerUI.MyInstance.ShowDescription(skillName);

    }

    public override void DeSelect() {
        base.DeSelect();
    }

}

}