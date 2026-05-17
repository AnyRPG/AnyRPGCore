using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class TradeSkillPrerequisite : ConfiguredClass, IPrerequisite {

        public event System.Action<UnitController> OnStatusUpdated = delegate { };

        [SerializeField]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;

        private Skill prerequisiteSkill = null;

        private string ownerName = null;

        // game manager references
        private SystemEventManager systemEventManager = null;

        public void UpdateStatus(UnitController sourceUnitController, bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = sourceUnitController.CharacterSkillManager.HasSkill(prerequisiteSkill);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated(sourceUnitController);
                }
            }
        }

        public void HandleSkillListChanged(UnitController unitController, Skill newSkill) {
            UpdateStatus(unitController);
        }

        public virtual bool IsMet(UnitController sourceUnitController) {
            return prerequisiteMet;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName) {
            this.ownerName = ownerName;
            Configure(systemGameManager);
            prerequisiteSkill = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                Skill tmpPrerequisiteSkill = systemDataFactory.GetResource<Skill>(prerequisiteName);
                if (tmpPrerequisiteSkill != null) {
                    prerequisiteSkill = tmpPrerequisiteSkill;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find skill : " + prerequisiteName + " while inititalizing a prerequisite for " + ownerName + ".  CHECK INSPECTOR");
                }
            }
            systemEventManager.OnLearnSkill += HandleSkillListChanged;
            systemEventManager.OnUnLearnSkill += HandleSkillListChanged;
        }

        public void CleanupScriptableObjects() {
            systemEventManager.OnLearnSkill -= HandleSkillListChanged;
            systemEventManager.OnUnLearnSkill -= HandleSkillListChanged;
        }
    }

}