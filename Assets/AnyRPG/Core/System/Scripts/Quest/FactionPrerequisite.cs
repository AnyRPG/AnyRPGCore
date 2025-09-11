using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class FactionPrerequisite : ConfiguredClass, IPrerequisite {


        public event System.Action<UnitController> OnStatusUpdated = delegate { };

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Faction))]
        private string prerequisiteName = string.Empty;

        [SerializeField]
        private float prerequisiteDisposition = 0f;


        private bool prerequisiteMet = false;

        private string ownerName = null;

        private Faction prerequisiteFaction = null;

        // game manager references
        private PlayerManager playerManager = null;
        private SystemEventManager systemEventManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void HandleReputationChange(UnitController sourceUnitController) {
            UpdateStatus(sourceUnitController);
        }

        public void UpdateStatus(UnitController unitController, bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = (Faction.RelationWith(unitController, prerequisiteFaction) >= prerequisiteDisposition);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated(unitController);
                }
            }
        }

        public virtual bool IsMet(UnitController sourceUnitController) {
            return prerequisiteMet;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName) {
            this.ownerName = ownerName;
            Configure(systemGameManager);
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                prerequisiteFaction = systemDataFactory.GetResource<Faction>(prerequisiteName);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find faction : " + prerequisiteName + " while inititalizing a faction prerequisite for " + ownerName + ".  CHECK INSPECTOR");
            }
            systemEventManager.OnReputationChange += HandleReputationChange;
        }

        public void CleanupScriptableObjects() {
            systemEventManager.OnReputationChange -= HandleReputationChange;
        }
    }

}