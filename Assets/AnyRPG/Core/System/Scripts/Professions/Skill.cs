using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Skill", menuName = "AnyRPG/Skill")]
    public class Skill : DescribableResource, IRewardable {

        [Header("Skill")]

        [SerializeField]
        private int requiredLevel = 1;

        [SerializeField]
        private bool autoLearn = false;

        [Tooltip("If true, the skill will level up with use.")]
        [SerializeField]
        private bool useSkillLevels = true;

        [Tooltip("A list of character levels and corresponding skill caps")]
        [SerializeField]
        private List<SkillLevelCapNode> skillLevelCapList = new List<SkillLevelCapNode>();

        [Tooltip("List of abilities that are learned when this skill is learned")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Ability))]
        private List<string> abilityNames = new List<string>();

        private List<AbilityProperties> abilityList = new List<AbilityProperties>();

        public int RequiredLevel { get => requiredLevel; }
        public bool AutoLearn { get => autoLearn; }
        public List<AbilityProperties> AbilityList { get => abilityList; set => abilityList = value; }
        public bool UseSkillLevels { get => useSkillLevels; set => useSkillLevels = value; }

        // game manager references
        //protected PlayerManager playerManager = null;

        /*
        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }
        */

        public override string GetSummary() {
            return string.Format("<color=#ffff00ff>{0}</color>\n\n{1}", resourceName, GetDescription());
        }

        public void GiveReward(UnitController sourceUnitController) {
            sourceUnitController.CharacterSkillManager.LearnSkill(this);
        }

        public bool HasReward(UnitController sourceUnitController) {
            return sourceUnitController.CharacterSkillManager.HasSkill(this);
        }

        public int GetSkillCapForLevel(int characterLevel) {
            // find the highest character level that is less than or equal to the input character level, and return the corresponding skill cap
            int skillCap = 1;
            SkillLevelCapNode highestLevelNode = null;
            foreach (SkillLevelCapNode skillLevelCapNode in skillLevelCapList) {
                if (highestLevelNode == null || skillLevelCapNode.CharacterLevel > highestLevelNode.CharacterLevel) {
                    highestLevelNode = skillLevelCapNode;
                }
            }
            if (highestLevelNode != null) {
                skillCap = highestLevelNode.SkillLevelCap;
            }
            return skillCap;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            abilityList = new List<AbilityProperties>();
            if (abilityNames != null) {
                foreach (string abilityName in abilityNames) {
                    Ability baseAbility = systemDataFactory.GetResource<Ability>(abilityName);
                    if (baseAbility != null) {
                        abilityList.Add(baseAbility.AbilityProperties);
                    } else {
                        Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability : " + abilityName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }
        }

    }
}