using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class AbilityPrerequisite : ConfiguredClass, IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };

        [SerializeField]
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;

        private BaseAbilityProperties prerequisiteAbility = null;

        // game manager references
        private PlayerManager playerManager = null;

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
            prerequisiteMet = playerManager.MyCharacter.CharacterAbilityManager.HasAbility(prerequisiteAbility);
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

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            prerequisiteAbility = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                prerequisiteAbility = systemDataFactory.GetResource<BaseAbility>(prerequisiteName).AbilityProperties;
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