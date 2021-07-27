using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class DialogPrerequisite : IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };


        [SerializeField]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;


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
            //Debug.Log("DialogPrerequisite.IsMet(): " + prerequisiteName);
            /*
            Dialog _dialog = SystemDialogManager.Instance.GetResource(prerequisiteName);
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
            prerequisiteDialog = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                Dialog tmpDialog = SystemDialogManager.Instance.GetResource(prerequisiteName);
                if (tmpDialog != null) {
                    prerequisiteDialog = tmpDialog;
                    prerequisiteDialog.OnDialogCompleted += HandleDialogCompleted;
                } else {
                    Debug.LogError("DialogPrerequisite.SetupScriptableObjects(): Could not find dialog : " + prerequisiteName + " while inititalizing a dialog prerequisite.  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("DialogPrerequisite.SetupScriptableObjects(): no prerequisite was defined while inititalizing a dialog prerequisite.  CHECK INSPECTOR");
            }
        }

        public void CleanupScriptableObjects() {
            if (prerequisiteDialog != null) {
                prerequisiteDialog.OnDialogCompleted -= HandleDialogCompleted;
            }
        }
    }

}