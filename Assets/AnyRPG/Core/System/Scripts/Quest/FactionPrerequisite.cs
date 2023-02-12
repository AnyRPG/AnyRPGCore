using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class FactionPrerequisite : ConfiguredClass, IPrerequisite {


        public event System.Action OnStatusUpdated = delegate { };


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

        public void HandleReputationChange(string eventName, EventParamProperties eventParam) {
            UpdateStatus();
        }

        public void UpdateStatus(bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = (Faction.RelationWith(playerManager.MyCharacter, prerequisiteFaction) >= prerequisiteDisposition);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated();
                }
            }
        }

        public virtual bool IsMet(BaseCharacter baseCharacter) {
            return prerequisiteMet;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName) {
            this.ownerName = ownerName;
            Configure(systemGameManager);
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                prerequisiteFaction = systemDataFactory.GetResource<Faction>(prerequisiteName);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find faction : " + prerequisiteName + " while inititalizing a faction prerequisite for " + ownerName + ".  CHECK INSPECTOR");
            }
            SystemEventManager.StartListening("OnReputationChange", HandleReputationChange);
        }

        public void CleanupScriptableObjects() {
            SystemEventManager.StopListening("OnReputationChange", HandleReputationChange);
        }
    }

}