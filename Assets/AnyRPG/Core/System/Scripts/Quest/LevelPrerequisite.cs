using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class LevelPrerequisite : ConfiguredClass, IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };

        [SerializeField]
        private int requiredLevel = 1;

        //private PrerequisiteConditions prerequisiteConditions = null;

        private bool prerequisiteMet = false;

        // game manager references
        private PlayerManager playerManager = null;
        private SystemEventManager systemEventManager = null;

        public void UpdateStatus(bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = (playerManager.MyCharacter.CharacterStats.Level >= requiredLevel);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated();
                }
            }
        }


        public void HandleLevelChanged(int newLevel) {
            UpdateStatus();
        }


        public virtual bool IsMet(BaseCharacter baseCharacter) {
            //Debug.Log("LevelPrerequisite.IsMet()");
            
            return prerequisiteMet;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            //this.prerequisiteConditions = prerequisiteConditions;
            systemEventManager.OnLevelChanged += HandleLevelChanged;
        }

        public void CleanupScriptableObjects() {
            systemEventManager.OnLevelChanged -= HandleLevelChanged;
        }

    }

}