using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class LevelPrerequisite : IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };

        [SerializeField]
        private int requiredLevel = 1;

        //private PrerequisiteConditions prerequisiteConditions = null;

        private bool prerequisiteMet = false;

        public void UpdateStatus(bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = (PlayerManager.Instance.MyCharacter.CharacterStats.Level >= requiredLevel);
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

        public void SetupScriptableObjects() {
            //this.prerequisiteConditions = prerequisiteConditions;
            SystemGameManager.Instance.EventManager.OnLevelChanged += HandleLevelChanged;
        }

        public void CleanupScriptableObjects() {
            if (SystemGameManager.Instance.EventManager != null) {
                SystemGameManager.Instance.EventManager.OnLevelChanged -= HandleLevelChanged;
            }
        }

    }

}