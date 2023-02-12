using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class VisitZonePrerequisite : ConfiguredClass, IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };

        [SerializeField]
        [ResourceSelector(resourceType = typeof(SceneNode))]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;

        private string ownerName = null;

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
            return prerequisiteMet;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName) {
            this.ownerName = ownerName;
            Configure(systemGameManager);
            prerequisiteSceneNode = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                SceneNode tmpPrerequisiteSceneNode = systemDataFactory.GetResource<SceneNode>(prerequisiteName);
                if (tmpPrerequisiteSceneNode != null) {
                    prerequisiteSceneNode = tmpPrerequisiteSceneNode;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find scene node : " + prerequisiteName + " while inititalizing a visit zone prerequisite for " + ownerName + ".  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): prerequisite empty while inititalizing a visit zone prerequisite for " + ownerName + ".  CHECK INSPECTOR");
            }
            prerequisiteSceneNode.OnVisitZone += HandleSceneNodeVisisted;
        }

        public void CleanupScriptableObjects() {
            prerequisiteSceneNode.OnVisitZone -= HandleSceneNodeVisisted;
        }
    }

}