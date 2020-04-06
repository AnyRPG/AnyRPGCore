using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class AbilityPrerequisite : IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };

        [SerializeField]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;


        private BaseAbility prerequisiteAbility = null;

        public void HandleAbilityListChanged(BaseAbility newAbility) {
            if (newAbility == prerequisiteAbility) {
                prerequisiteMet = true;
                OnStatusUpdated();
            } else {
                //prerequisiteMet = false;
            }
        }

        public void UpdateStatus() {
            bool originalResult = prerequisiteMet;
            prerequisiteMet = PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(prerequisiteAbility);
            if (prerequisiteMet != originalResult) {
                OnStatusUpdated();
            }
        }


        public virtual bool IsMet(BaseCharacter baseCharacter) {
            //Debug.Log("AbilityPrerequisite.IsMet()");
            return prerequisiteMet;
        }

        public void SetupScriptableObjects() {
            prerequisiteAbility = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                prerequisiteAbility = SystemAbilityManager.MyInstance.GetResource(prerequisiteName);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + prerequisiteName + " while inititalizing a prerequisite.  CHECK INSPECTOR");
            }
            SystemEventManager.MyInstance.OnAbilityListChanged += HandleAbilityListChanged;
        }

        public void CleanupScriptableObjects() {
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnAbilityListChanged -= HandleAbilityListChanged;
            }
        }

    }

}