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


        private SceneNode prerequisiteSceneNode = null;

        // game manager references
        private SystemDataFactory systemDataFactory = null;

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

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            prerequisiteSceneNode = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                SceneNode tmpPrerequisiteSceneNode = systemDataFactory.GetResource<SceneNode>(prerequisiteName);
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