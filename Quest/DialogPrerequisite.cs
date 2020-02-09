using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class DialogPrerequisite : IPrerequisite {

        [SerializeField]
        private string prerequisiteName;

        private Dialog prerequisiteDialog = null;

        public virtual bool IsMet(BaseCharacter baseCharacter) {
            Debug.Log("DialogPrerequisite.IsMet(): " + prerequisiteName);
            Dialog _dialog = SystemDialogManager.MyInstance.GetResource(prerequisiteName);
            if (_dialog != null) {
                if (_dialog.TurnedIn == true) {
                    return true;
                }
            }
            return false;
        }

        public void SetupScriptableObjects() {
            prerequisiteDialog = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                prerequisiteDialog = SystemDialogManager.MyInstance.GetResource(prerequisiteName);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find dialog : " + prerequisiteName + " while inititalizing a dialog prerequisite.  CHECK INSPECTOR");
            }
        }

    }

}