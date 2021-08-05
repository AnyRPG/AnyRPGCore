using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class FactionPrerequisite : IPrerequisite {


        public event System.Action OnStatusUpdated = delegate { };


        [SerializeField]
        [ResourceSelector(resourceType = typeof(Faction))]
        private string prerequisiteName = string.Empty;

        [SerializeField]
        private float prerequisiteDisposition = 0f;


        private bool prerequisiteMet = false;


        private Faction prerequisiteFaction = null;

        public void HandleReputationChange(string eventName, EventParamProperties eventParam) {
            UpdateStatus();
        }

        public void UpdateStatus(bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = (Faction.RelationWith(SystemGameManager.Instance.PlayerManager.MyCharacter, prerequisiteFaction) >= prerequisiteDisposition);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated();
                }
            }
        }

        public virtual bool IsMet(BaseCharacter baseCharacter) {
            //Debug.Log("DialogPrerequisite.IsMet(): " + prerequisiteName);
            /*
            Dialog _dialog = SystemDataFactory.Instance.GetResource<Dialog>(prerequisiteName);
            if (_dialog != null) {
                if (_dialog.TurnedIn == true) {
                    return true;
                }
            }
            return false;
            */
            return prerequisiteMet;
        }

        public void SetupScriptableObjects() {
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                prerequisiteFaction = SystemDataFactory.Instance.GetResource<Faction>(prerequisiteName);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find dialog : " + prerequisiteName + " while inititalizing a dialog prerequisite.  CHECK INSPECTOR");
            }
            SystemEventManager.StartListening("OnReputationChange", HandleReputationChange);
        }

        public void CleanupScriptableObjects() {
            if (SystemGameManager.Instance.SystemEventManager != null) {
                SystemEventManager.StopListening("OnReputationChange", HandleReputationChange);
            }
        }
    }

}