using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[System.Serializable]
public class AbilityPrerequisite : IPrerequisite {

    [SerializeField]
    private string prerequisiteName;

    public virtual bool IsMet(BaseCharacter baseCharacter) {
        //Debug.Log("AbilityPrerequisite.IsMet()");
        if (baseCharacter == null) {
            //Debug.Log("AbilityPrerequisite.IsMet(): baseCharacter is null!");
            return false;
        }
        if (baseCharacter.MyCharacterAbilityManager == null) {
            //Debug.Log("AbilityPrerequisite.IsMet(): baseCharacter.MyCharacterAbilityManager is null!");
            return false;
        }
        if (baseCharacter.MyCharacterAbilityManager.MyAbilityList == null) {
            //Debug.Log("AbilityPrerequisite.IsMet(): baseCharacter.MyCharacterAbilityManager.MySkillList is null!");
            return false;
        }
        if (baseCharacter.MyCharacterAbilityManager.HasAbility(prerequisiteName)) {
            //Debug.Log("AbilityPrerequisite.IsMet; " + prerequisiteName + "; abilitymanager has ability. returning TRUE");
            return true;
        }

        //Debug.Log("AbilityPrerequisite.IsMet; " + prerequisiteName + "returning FALSE");
        return false;
    }
}

}