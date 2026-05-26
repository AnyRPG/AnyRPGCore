using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class LevelPrerequisite : ConfiguredClass, IPrerequisite {

        public event System.Action<UnitController> OnStatusUpdated = delegate { };

        [SerializeField]
        private int requiredLevel = 1;

        //private PrerequisiteConditions prerequisiteConditions = null;

        private bool prerequisiteMet = false;

        private string ownerName = null;

        // game manager references
        private SystemEventManager systemEventManager = null;

        public void UpdateStatus(UnitController sourceUnitController, bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = (sourceUnitController.CharacterStats.Level >= requiredLevel);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated(sourceUnitController);
                }
            }
        }

        public void HandleLevelChanged(UnitController unitController, int newLevel) {
            UpdateStatus(unitController);
        }

        public virtual bool IsMet(UnitController sourceUnitController) {
            //Debug.Log("LevelPrerequisite.IsMet()");

            return prerequisiteMet;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName) {
            this.ownerName = ownerName;
            Configure(systemGameManager);
            //this.prerequisiteConditions = prerequisiteConditions;
            systemEventManager.OnLevelChanged += HandleLevelChanged;
        }

        public void CleanupScriptableObjects() {
            systemEventManager.OnLevelChanged -= HandleLevelChanged;
        }

    }

}