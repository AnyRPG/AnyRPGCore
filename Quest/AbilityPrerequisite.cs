using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class AbilityPrerequisite : IPrerequisite {

        [SerializeField]
        private string prerequisiteName;

        private BaseAbility prerequisiteAbility;

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
            if (baseCharacter.MyCharacterAbilityManager.HasAbility(prerequisiteAbility)) {
                //Debug.Log("AbilityPrerequisite.IsMet; " + prerequisiteName + "; abilitymanager has ability. returning TRUE");
                return true;
            }

            //Debug.Log("AbilityPrerequisite.IsMet; " + prerequisiteName + "returning FALSE");
            return false;
        }

        public void SetupScriptableObjects() {
            prerequisiteAbility = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                prerequisiteAbility = SystemAbilityManager.MyInstance.GetResource(prerequisiteName);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + prerequisiteName + " while inititalizing a prerequisite.  CHECK INSPECTOR");
            }
        }

    }

}