using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class TradeSkillPrerequisite : ConfiguredClass, IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };

        [SerializeField]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;

        private Skill prerequisiteSkill = null;

        // game manager references
        private PlayerManager playerManager = null;
        private SystemEventManager systemEventManager = null;

        public void UpdateStatus(bool notify = true) {
            bool originalResult = prerequisiteMet;
            bool checkResult = playerManager.MyCharacter.CharacterSkillManager.HasSkill(prerequisiteSkill);
            if (checkResult != originalResult) {
                prerequisiteMet = checkResult;
                if (notify == true) {
                    OnStatusUpdated();
                }
            }
        }

        public void HandleSkillListChanged(Skill newSkill) {
            UpdateStatus();
        }

        public virtual bool IsMet(BaseCharacter baseCharacter) {
            return prerequisiteMet;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            prerequisiteSkill = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                Skill tmpPrerequisiteSkill = systemDataFactory.GetResource<Skill>(prerequisiteName);
                if (tmpPrerequisiteSkill != null) {
                    prerequisiteSkill = tmpPrerequisiteSkill;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find skill : " + prerequisiteName + " while inititalizing a prerequisite.  CHECK INSPECTOR");
                }
            }
            systemEventManager.OnSkillListChanged += HandleSkillListChanged;
        }

        public void CleanupScriptableObjects() {
            systemEventManager.OnSkillListChanged -= HandleSkillListChanged;
        }
    }

}