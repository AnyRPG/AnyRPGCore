using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CharacterClassPrerequisite : ConfiguredClass, IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };

        [SerializeField]
        [ResourceSelector(resourceType = typeof(CharacterClass))]
        private string requiredCharacterClass = string.Empty;


        private bool prerequisiteMet = false;

        private CharacterClass prerequisiteCharacterClass = null;

        // game manager references
        private PlayerManager playerManager = null;
        private SystemEventManager systemEventManager = null;

        public void UpdateStatus(bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = (playerManager.MyCharacter.CharacterClass == prerequisiteCharacterClass);
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

        public override void SetGameManagerReferences() {
            //Debug.Log("CharacterClassPrerequisite.SetGameManagerReferences()");
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            //Debug.Log("CharacterClassPrerequisite.SetupScriptableObjects(" + (systemGameManager == null ? "null" : systemGameManager.gameObject.name) + ")");

            Configure(systemGameManager);
            prerequisiteCharacterClass = null;
            if (requiredCharacterClass != null && requiredCharacterClass != string.Empty) {
                prerequisiteCharacterClass = systemDataFactory.GetResource<CharacterClass>(requiredCharacterClass);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + prerequisiteCharacterClass + " while inititalizing a character class prerequisite.  CHECK INSPECTOR");
            }

            systemEventManager.OnClassChange += HandleClassChange;
        }

        public void CleanupScriptableObjects() {
            systemEventManager.OnClassChange -= HandleClassChange;
        }
    }

}