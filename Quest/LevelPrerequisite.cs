using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelPrerequisite : IPrerequisite {

    [SerializeField]
    private int requiredLevel;

    public virtual bool IsMet(BaseCharacter baseCharacter) {
        //Debug.Log("LevelPrerequisite.IsMet()");
        if (baseCharacter == null) {
            //Debug.Log("LevelPrerequisite.IsMet(): baseCharacter is null!!");
            return false;
        }
        if (baseCharacter.MyCharacterStats == null) {
            //Debug.Log("LevelPrerequisite.IsMet(): baseCharacter.MyCharacterStats is null!!");
            return false;
        }
        if (baseCharacter.MyCharacterStats.MyLevel >= requiredLevel) {
            return true;
        }
        return false;
    }
}
