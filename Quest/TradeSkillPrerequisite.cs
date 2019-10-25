using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[System.Serializable]
public class TradeSkillPrerequisite : IPrerequisite {

    [SerializeField]
    private string prerequisiteName;

    public virtual bool IsMet(BaseCharacter baseCharacter) {
        //Debug.Log("TradeSkillPrerequisite.IsMet()");
        if (baseCharacter == null) {
            //Debug.Log("TradeSkillPrerequisite.IsMet(): baseCharacter is null!");
            return false;
        }
        if (baseCharacter.MyCharacterSkillManager == null) {
            //Debug.Log("TradeSkillPrerequisite.IsMet(): baseCharacter.MyCharacterSkillManager is null!");
            return false;
        }
        if (baseCharacter.MyCharacterSkillManager.MySkillList.Count == 0) {
            //Debug.Log("TradeSkillPrerequisite.IsMet(): baseCharacter.MyCharacterSkillManager.MySkillList is null!");
            return false;
        }
        if (baseCharacter.MyCharacterSkillManager.HasSkill(prerequisiteName)) {
            return true;
        }
        return false;
    }
}

}