using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill")]
public class Skill : DescribableResource {

    [SerializeField]
    private int requiredLevel = 1;

    [SerializeField]
    private bool autoLearn = false;

    [SerializeField]
    private List<BaseAbility> abilityList = new List<BaseAbility>();

    public int MyRequiredLevel { get => requiredLevel; }
    public bool MyAutoLearn { get => autoLearn; }
    public List<BaseAbility> MyAbilityList { get => abilityList; set => abilityList = value; }

    public override string GetDescription() {
        return string.Format("<color=#ffff00ff>{0}</color>\n\n{1}", resourceName, GetSummary());
    }

}
}