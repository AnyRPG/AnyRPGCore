using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CharacterClassPrerequisite : ConfiguredClass, IPrerequisite {

        public event System.Action<UnitController> OnStatusUpdated = delegate { };

        [SerializeField]
        [ResourceSelector(resourceType = typeof(CharacterClass))]
        private string requiredCharacterClass = string.Empty;


        private bool prerequisiteMet = false;

        private CharacterClass prerequisiteCharacterClass = null;

        private string ownerName = null;

        // game manager references
        private PlayerManagerClient playerManager = null;
        private SystemEventManager systemEventManager = null;

        public void UpdateStatus(UnitController unitController, bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = (unitController.BaseCharacter.CharacterClass == prerequisiteCharacterClass);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated(unitController);
                }
            }
        }


        public void HandleClassChange(UnitController sourceUnitController, CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            //Debug.Log("CharacterClassPrerequisite.HandleClassChange()");
            UpdateStatus(sourceUnitController);
        }


        public virtual bool IsMet(UnitController sourceUnitController) {
            //Debug.Log("LevelPrerequisite.IsMet()");
            return prerequisiteMet;
        }

        public override void SetGameManagerReferences() {
            //Debug.Log("CharacterClassPrerequisite.SetGameManagerReferences()");
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName) {
            //Debug.Log("CharacterClassPrerequisite.SetupScriptableObjects(" + (systemGameManager == null ? "null" : systemGameManager.gameObject.name) + ")");

            this.ownerName = ownerName;
            Configure(systemGameManager);
            prerequisiteCharacterClass = null;
            if (requiredCharacterClass != null && requiredCharacterClass != string.Empty) {
                prerequisiteCharacterClass = systemDataFactory.GetResource<CharacterClass>(requiredCharacterClass);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + prerequisiteCharacterClass + " while inititalizing a character class prerequisite for " + ownerName + ".  CHECK INSPECTOR");
            }

            systemEventManager.OnClassChange += HandleClassChange;
        }

        public void CleanupScriptableObjects() {
            systemEventManager.OnClassChange -= HandleClassChange;
        }
    }

}