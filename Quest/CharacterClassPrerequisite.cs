using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CharacterClassPrerequisite : IPrerequisite {

        [SerializeField]
        private string requiredCharacterClass;

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
            if (SystemResourceManager.MatchResource(requiredCharacterClass, baseCharacter.MyCharacterClassName)) {
                return true;
            }
            return false;
        }
    }

}