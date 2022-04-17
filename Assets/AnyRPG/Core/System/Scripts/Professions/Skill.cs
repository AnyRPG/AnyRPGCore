using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Skill", menuName = "AnyRPG/Skills/Skill")]
    public class Skill : DescribableResource, IRewardable {

        [Header("Skill")]

        [SerializeField]
        private int requiredLevel = 1;

        [SerializeField]
        private bool autoLearn = false;

        [Tooltip("List of abilities that are learned when this skill is learned")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        private List<string> abilityNames = new List<string>();

        private List<BaseAbilityProperties> abilityList = new List<BaseAbilityProperties>();

        public int RequiredLevel { get => requiredLevel; }
        public bool AutoLearn { get => autoLearn; }
        public List<BaseAbilityProperties> AbilityList { get => abilityList; set => abilityList = value; }

        // game manager references
        protected PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public override string GetSummary() {
            return string.Format("<color=#ffff00ff>{0}</color>\n\n{1}", resourceName, GetDescription());
        }

        public void GiveReward() {
            playerManager.MyCharacter.CharacterSkillManager.LearnSkill(this);
        }

        public bool HasReward() {
            return playerManager.MyCharacter.CharacterSkillManager.HasSkill(this);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            abilityList = new List<BaseAbilityProperties>();
            if (abilityNames != null) {
                foreach (string abilityName in abilityNames) {
                    BaseAbility baseAbility = systemDataFactory.GetResource<BaseAbility>(abilityName);
                    if (baseAbility != null) {
                        abilityList.Add(baseAbility.AbilityProperties);
                    } else {
                        Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability : " + abilityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }
        }

    }
}