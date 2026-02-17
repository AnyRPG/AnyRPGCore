using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class AbilityPrerequisite : ConfiguredClass, IPrerequisite {

        public event System.Action<UnitController> OnStatusUpdated = delegate { };

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Ability))]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;

        private AbilityProperties prerequisiteAbility = null;

        private string ownerName = null;

        // game manager references
        private PlayerManagerClient playerManager = null;

        public void HandleAbilityListChanged(UnitController unitController) {
            //Debug.Log("AbilityPrerequisite.HandleAbilityListChanged()");
            bool originalResult = prerequisiteMet;
            prerequisiteMet = true;
            if (prerequisiteMet != originalResult) {
                OnStatusUpdated(unitController);
            }
        }

        public void UpdateStatus(UnitController sourceUnitController, bool notify = true) {
            bool originalResult = prerequisiteMet;
            prerequisiteMet = sourceUnitController.CharacterAbilityManager.HasAbility(prerequisiteAbility);
            if (prerequisiteMet != originalResult) {
                if (notify == true) {
                    OnStatusUpdated(sourceUnitController);
                }
            }
        }

        public virtual bool IsMet(UnitController sourceUnitController) {
            //Debug.Log("AbilityPrerequisite.IsMet()");
            return prerequisiteMet;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName) {
            this.ownerName = ownerName;
            Configure(systemGameManager);
            prerequisiteAbility = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                prerequisiteAbility = systemDataFactory.GetResource<Ability>(prerequisiteName).AbilityProperties;
                if (prerequisiteAbility != null) {
                    prerequisiteAbility.OnAbilityLearn += HandleAbilityListChanged;
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + prerequisiteName + " while inititalizing a prerequisite for " + ownerName + ".  CHECK INSPECTOR");
            }
        }

        public void CleanupScriptableObjects() {
            if (prerequisiteAbility != null) {
                prerequisiteAbility.OnAbilityLearn -= HandleAbilityListChanged;
            }
        }

    }

}