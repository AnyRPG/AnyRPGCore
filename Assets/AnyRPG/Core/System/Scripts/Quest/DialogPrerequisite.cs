using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class DialogPrerequisite : ConfiguredClass, IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };


        [SerializeField]
        [ResourceSelector(resourceType = typeof(Dialog))]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;

        private string ownerName = null;

        private Dialog prerequisiteDialog = null;

        public void UpdateStatus(bool notify = true) {
            bool originalResult = prerequisiteMet;
            //bool checkResult = (prerequisiteDialog.TurnedIn == true);
            // updated to prevent repeatable dialogs from trigger prerequisites in cutscenes and doing things before they should.
            bool checkResult = (prerequisiteDialog.TurnedIn == true && prerequisiteDialog.Repeatable == false);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated();
                }
            }
        }


        public void HandleDialogCompleted() {
            prerequisiteMet = true;
            OnStatusUpdated();
        }

        public virtual bool IsMet(BaseCharacter baseCharacter) {
            return prerequisiteMet;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName) {
            this.ownerName = ownerName;
            Configure(systemGameManager);
            prerequisiteDialog = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                Dialog tmpDialog = systemDataFactory.GetResource<Dialog>(prerequisiteName);
                if (tmpDialog != null) {
                    prerequisiteDialog = tmpDialog;
                    prerequisiteDialog.OnDialogCompleted += HandleDialogCompleted;
                } else {
                    Debug.LogError("DialogPrerequisite.SetupScriptableObjects(): Could not find dialog : " + prerequisiteName + " while inititalizing a dialog prerequisite for " + ownerName + ".  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("DialogPrerequisite.SetupScriptableObjects(): no prerequisite was defined while inititalizing a dialog prerequisite for " + ownerName + ".  CHECK INSPECTOR");
            }
        }

        public void CleanupScriptableObjects() {
            if (prerequisiteDialog != null) {
                prerequisiteDialog.OnDialogCompleted -= HandleDialogCompleted;
            }
        }
    }

}