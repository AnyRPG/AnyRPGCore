using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CharacterClassPrerequisite : IPrerequisite {

        [SerializeField]
        private string requiredCharacterClass = string.Empty;

        private CharacterClass prerequisiteCharacterClass = null;

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
            if (prerequisiteCharacterClass == baseCharacter.MyCharacterClass) {
                return true;
            }
            return false;
        }

        public void SetupScriptableObjects() {
            prerequisiteCharacterClass = null;
            if (requiredCharacterClass != null && requiredCharacterClass != string.Empty) {
                prerequisiteCharacterClass = SystemCharacterClassManager.MyInstance.GetResource(requiredCharacterClass);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + prerequisiteCharacterClass + " while inititalizing a character class prerequisite.  CHECK INSPECTOR");
            }
        }

    }

}