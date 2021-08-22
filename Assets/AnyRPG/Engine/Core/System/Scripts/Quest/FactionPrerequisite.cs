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


        private Faction prerequisiteFaction = null;

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private PlayerManager playerManager = null;
        //private SystemEventManager systemEventManager = null;

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
            systemDataFactory = systemGameManager.SystemDataFactory;
            //playerManager = systemGameManager.PlayerManager;
            //systemEventManager = systemGameManager.SystemEventManager;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                prerequisiteFaction = systemDataFactory.GetResource<Faction>(prerequisiteName);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find dialog : " + prerequisiteName + " while inititalizing a dialog prerequisite.  CHECK INSPECTOR");
            }
            SystemEventManager.StartListening("OnReputationChange", HandleReputationChange);
        }

        public void CleanupScriptableObjects() {
            SystemEventManager.StopListening("OnReputationChange", HandleReputationChange);
        }
    }

}