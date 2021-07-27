using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class VisitZonePrerequisite : IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };


        [SerializeField]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;


        private SceneNode prerequisiteSceneNode = null;

        public void UpdateStatus(bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = (prerequisiteSceneNode.Visited == true);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated();
                }
            }
        }


        public void HandleSceneNodeVisisted() {
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
            prerequisiteSceneNode = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                SceneNode tmpPrerequisiteSceneNode = SystemSceneNodeManager.Instance.GetResource(prerequisiteName);
                if (tmpPrerequisiteSceneNode != null) {
                    prerequisiteSceneNode = tmpPrerequisiteSceneNode;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find scene node : " + prerequisiteName + " while inititalizing a visit zone prerequisite.  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): prerequisite empty while inititalizing a visit zone prerequisite.  CHECK INSPECTOR");
            }
            prerequisiteSceneNode.OnVisitZone += HandleSceneNodeVisisted;
        }

        public void CleanupScriptableObjects() {
            prerequisiteSceneNode.OnVisitZone -= HandleSceneNodeVisisted;
        }
    }

}