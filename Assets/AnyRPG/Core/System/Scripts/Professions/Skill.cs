using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Skill", menuName = "AnyRPG/Skill")]
    public class Skill : DescribableResource, IRewardable {

        private const int fallbackExperienceChartValue = 100;

        [Header("Skill")]

        [SerializeField]
        private int requiredLevel = 1;

        [SerializeField]
        private bool autoLearn = false;

        [Tooltip("If true, the character will gain experience when they use this skill")]
        [SerializeField]
        private bool giveCharacterExperience = false;

        [Tooltip("If true, the skill will level up with use.")]
        [SerializeField]
        private bool useSkillLevels = true;

        [Tooltip("A list of character levels and corresponding skill caps")]
        [SerializeField]
        private List<SkillLevelCapNode> skillLevelCapList = new List<SkillLevelCapNode>();

        [Tooltip("If true, the skill will gain experience when used, and will level up when it reaches the experience required to level up.")]
        [SerializeField]
        private bool useSkillExperience = true;

        [Tooltip("A list of experience amounts required to level up the skill.  The index of the list is the skill level, and the value is the experience required to reach the next level.")]
        [SerializeField]
        private List<int> skillExperienceChart = new List<int>() {
            500,1000,1500,2000,2500,3500,4500,5500,6500,7500,
            8500,10000,11500,13000,14500,16000,17500,19000,21000,23000,
            25000,27000,29000,31500,34000,36500,39000,41500,44000,47000,
            50000,54000,58000,62000,66000,70000,75000,80000,85000,90000,
            95000,100000,105000,110000,116000,122000,128000,134000,140000,145000,
            151000,158000,165000,172000,179500,187000,194500,202000,209500,217000};

        [Tooltip("List of abilities that are learned when this skill is learned")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Ability))]
        private List<string> abilityNames = new List<string>();

        private List<AbilityProperties> abilityList = new List<AbilityProperties>();

        public int RequiredLevel { get => requiredLevel; }
        public bool AutoLearn { get => autoLearn; }
        public List<AbilityProperties> AbilityList { get => abilityList; set => abilityList = value; }
        public bool UseSkillLevels { get => useSkillLevels; set => useSkillLevels = value; }
        public bool GiveCharacterExperience { get => giveCharacterExperience; set => giveCharacterExperience = value; }
        public bool UseSkillExperience { get => useSkillExperience; set => useSkillExperience = value; }
        public List<int> SkillExperienceChart { get => skillExperienceChart; set => skillExperienceChart = value; }

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
                if (highestLevelNode == null || (skillLevelCapNode.CharacterLevel > highestLevelNode.CharacterLevel && characterLevel >= skillLevelCapNode.CharacterLevel)) {
                    highestLevelNode = skillLevelCapNode;
                }
            }
            if (highestLevelNode != null) {
                skillCap = highestLevelNode.SkillLevelCap;
            }
            return skillCap;
        }

        public int GetExperienceRequiredForLevel(int skillLevel) {
            if (skillExperienceChart.Count == 0) {
                // give a default fallback amount if no chart is set up
                return fallbackExperienceChartValue;
            }
            // if the level is within the experience chart, return the corresponding value
            if (skillLevel <= skillExperienceChart.Count) {
                return skillExperienceChart[skillLevel -1];
            }
            // if the level is higher than the experience chart, use the last value in the chart
            return skillExperienceChart[skillExperienceChart.Count - 1];
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