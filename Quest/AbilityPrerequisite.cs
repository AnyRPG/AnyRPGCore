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

        public void HandleAbilityListChanged() {
            //Debug.Log("AbilityPrerequisite.HandleAbilityListChanged()");
            bool originalResult = prerequisiteMet;
            prerequisiteMet = true;
            if (prerequisiteMet != originalResult) {
                OnStatusUpdated();
            }
        }

        public void UpdateStatus(bool notify = true) {
            bool originalResult = prerequisiteMet;
            prerequisiteMet = PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.HasAbility(prerequisiteAbility);
            if (prerequisiteMet != originalResult) {
                if (notify == true) {
                    OnStatusUpdated();
                }
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
                if (prerequisiteAbility != null) {
                    prerequisiteAbility.OnAbilityLearn += HandleAbilityListChanged;
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + prerequisiteName + " while inititalizing a prerequisite.  CHECK INSPECTOR");
            }
        }

        public void CleanupScriptableObjects() {
            if (prerequisiteAbility != null) {
                prerequisiteAbility.OnAbilityLearn -= HandleAbilityListChanged;
            }
        }

    }

}