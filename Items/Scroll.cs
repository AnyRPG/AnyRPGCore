using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New Scroll",menuName = "AnyRPG/Inventory/Items/Scroll", order = 1)]
public class Scroll : CastableItem {

    /*
    public override void Use() {
        //Debug.Log("Scroll.Use()");
        base.Use();
        PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
        Remove();
    }
    */

    public override string GetSummary() {
        string abilityName = "Ability Not Set In Inspector!";
        if (ability != null) {
            abilityName = ability.MyDisplayName;
        }
        return string.Format("{0}\n<color=green>Use: Cast {1}</color>", base.GetSummary(), abilityName);
    }


}

}