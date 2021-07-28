using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CharacterClassPrerequisite : IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };

        [SerializeField]
        private string requiredCharacterClass = string.Empty;


        private bool prerequisiteMet = false;

        private CharacterClass prerequisiteCharacterClass = null;

        public void UpdateStatus(bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = (PlayerManager.Instance.MyCharacter.CharacterClass == prerequisiteCharacterClass);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated();
                }
            }
        }


        public void HandleClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            //Debug.Log("CharacterClassPrerequisite.HandleClassChange()");
            UpdateStatus();
        }


        public virtual bool IsMet(BaseCharacter baseCharacter) {
            //Debug.Log("LevelPrerequisite.IsMet()");
            return prerequisiteMet;
        }

        public void SetupScriptableObjects() {
            prerequisiteCharacterClass = null;
            if (requiredCharacterClass != null && requiredCharacterClass != string.Empty) {
                prerequisiteCharacterClass = SystemCharacterClassManager.Instance.GetResource(requiredCharacterClass);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + prerequisiteCharacterClass + " while inititalizing a character class prerequisite.  CHECK INSPECTOR");
            }

            SystemGameManager.Instance.EventManager.OnClassChange += HandleClassChange;
        }

        public void CleanupScriptableObjects() {
            if (SystemGameManager.Instance.EventManager != null) {
                SystemGameManager.Instance.EventManager.OnClassChange -= HandleClassChange;
            }
        }
    }

}