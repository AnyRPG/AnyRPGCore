using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class FactionPrerequisite : IPrerequisite {


        public event System.Action OnStatusUpdated = delegate { };


        [SerializeField]
        private string prerequisiteName = string.Empty;

        [SerializeField]
        private float prerequisiteDisposition = 0f;


        private bool prerequisiteMet = false;


        private Faction prerequisiteFaction = null;

        public void HandleReputationChange() {
            UpdateStatus();
        }

        public void UpdateStatus() {
            bool originalResult = prerequisiteMet;
            bool checkResult = (Faction.RelationWith(PlayerManager.MyInstance.MyCharacter, prerequisiteFaction) >= prerequisiteDisposition);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                OnStatusUpdated();
            }
        }

        public virtual bool IsMet(BaseCharacter baseCharacter) {
            //Debug.Log("DialogPrerequisite.IsMet(): " + prerequisiteName);
            /*
            Dialog _dialog = SystemDialogManager.MyInstance.GetResource(prerequisiteName);
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
                prerequisiteFaction = SystemFactionManager.MyInstance.GetResource(prerequisiteName);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find dialog : " + prerequisiteName + " while inititalizing a dialog prerequisite.  CHECK INSPECTOR");
            }
            SystemEventManager.MyInstance.OnReputationChange += HandleReputationChange;
        }

        public void CleanupScriptableObjects() {
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnReputationChange -= HandleReputationChange;
            }
        }
    }

}