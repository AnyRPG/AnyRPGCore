using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class TradeSkillPrerequisite : IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };

        [SerializeField]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;

        private Skill prerequisiteSkill = null;

        public void UpdateStatus(bool notify = true) {
            //Debug.Log("TradeSkillPrerequisite.UpdateStatus(): " + (prerequisiteSkill != null ? prerequisiteSkill.MyName : "null") + "; originalResult: " + prerequisiteMet);
            bool originalResult = prerequisiteMet;
            bool checkResult = PlayerManager.MyInstance.MyCharacter.CharacterSkillManager.HasSkill(prerequisiteSkill);
            //Debug.Log("TradeSkillPrerequisite.UpdateStatus(): checkResult: " + checkResult);
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
            //Debug.Log("TradeSkillPrerequisite.IsMet()");
            return prerequisiteMet;
        }

        public void SetupScriptableObjects() {
            prerequisiteSkill = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                Skill tmpPrerequisiteSkill = SystemSkillManager.MyInstance.GetResource(prerequisiteName);
                if (tmpPrerequisiteSkill != null) {
                    prerequisiteSkill = tmpPrerequisiteSkill;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find skill : " + prerequisiteName + " while inititalizing a prerequisite.  CHECK INSPECTOR");
                }
            }
            SystemEventManager.MyInstance.OnSkillListChanged += HandleSkillListChanged;
        }

        public void CleanupScriptableObjects() {
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnSkillListChanged -= HandleSkillListChanged;
            }
        }
    }

}