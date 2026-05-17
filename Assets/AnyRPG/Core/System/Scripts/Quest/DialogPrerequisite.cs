using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class DialogPrerequisite : ConfiguredClass, IPrerequisite {

        public event System.Action<UnitController> OnStatusUpdated = delegate { };


        [SerializeField]
        [ResourceSelector(resourceType = typeof(Dialog))]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;

        private string ownerName = null;

        private Dialog prerequisiteDialog = null;

        public void UpdateStatus(UnitController sourceUnitController, bool notify = true) {
            //Debug.Log($"{ownerName}.DialogPrerequisite.UpdateStatus()");

            bool originalResult = prerequisiteMet;
            //bool checkResult = (prerequisiteDialog.TurnedIn == true);
            // updated to prevent repeatable dialogs from trigger prerequisites in cutscenes and doing things before they should.
            bool checkResult = (prerequisiteDialog.TurnedIn(sourceUnitController) == true && prerequisiteDialog.Repeatable == false);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated(sourceUnitController);
                }
            }
        }


        public void HandleDialogCompleted(UnitController unitController) {
            //Debug.Log($"{ownerName}.DialogPrerequisite.HandleDialogCompleted({unitController.gameObject.name})");

            prerequisiteMet = true;
            OnStatusUpdated(unitController);
        }

        public virtual bool IsMet(UnitController sourceUnitController) {
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