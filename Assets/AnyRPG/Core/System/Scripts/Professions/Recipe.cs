using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Recipe", menuName = "AnyRPG/Recipe")]
    public class Recipe : DescribableResource {

        [Header("Recipe")]

        [Tooltip("If true, this recipe is automatically learned at the appropriate level")]
        [SerializeField]
        private bool autoLearn = false;

        [Tooltip("The level that is required to learn this recipe")]
        [SerializeField]
        private int requiredLevel = 1;

        [Tooltip("The skill required to gather from this node, or empty for none.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Skill))]
        private string skillName = string.Empty;

        [Tooltip("The required skill level to learn this recipe.")]
        [SerializeField]
        private int requiredSkillLevel = 0;

        [Tooltip("The chance to gain a skill level when crafting this recipe.  1 = 100% chance, 0.5 = 50% chance, etc.  This only applies if skill experience is not in use for this recipe skill.")]
        [SerializeField]
        private float chanceToGainLevel = 1f;

        [Tooltip("The amount of skill experience to give when crafting this recipe.")]
        [SerializeField]
        private int skillExperienceReward = 25;

        [Tooltip("The maximum skill level at which skill experience will be granted for crafting this recipe.  If the character skill is higher than this level, they will get no skill experience. 0 means this recipe will never stop giving experience")]
        [SerializeField]
        private int maxSkillExperienceLevel = 0;

        [Tooltip("The amount of character experience to give when crafting this recipe.")]
        [SerializeField]
        private int characterExperienceReward = 25;

        [Tooltip("The maximum character level at which experience will be granted for crafting this recipe.  If the character is higher than this level, they will get no experience. 0 means this recipe will never stop giving experience")]
        [SerializeField]
        private int maxCharacterExperienceLevel = 0;

        [Header("Crafting")]

        [SerializeField]
        private List<CraftingMaterial> craftingMaterials = new List<CraftingMaterial>();

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        private string itemOutputName = string.Empty;

        //[SerializeField]
        private Item output;

        [SerializeField]
        private int outputCount = 0;

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Ability))]
        private string craftAbilityName = string.Empty;

        [Header("Prefabs")]

        [Tooltip("Physical prefabs to attach to bones on the character unit")]
        [SerializeField]
        private List<AbilityAttachmentNode> holdableObjectList = new List<AbilityAttachmentNode>();

        // a reference to the actual skill
        private Skill skill;

        // a reference to the actual craft ability
        private CraftAbilityProperties craftAbility;

        public Item Output { get => output; set => output = value; }
        public List<CraftingMaterial> CraftingMaterials { get => craftingMaterials; set => craftingMaterials = value; }
        public int OutputCount { get => outputCount; set => outputCount = value; }
        public CraftAbilityProperties CraftAbility { get => craftAbility; set => craftAbility = value; }
        public Skill Skill { get => skill; set => skill = value; }
        public bool AutoLearn { get => autoLearn; set => autoLearn = value; }
        public int RequiredLevel { get => requiredLevel; set => requiredLevel = value; }
        public List<AbilityAttachmentNode> HoldableObjectList { get => holdableObjectList; set => holdableObjectList = value; }
        public int MaxCharacterExperienceLevel { get => maxCharacterExperienceLevel; set => maxCharacterExperienceLevel = value; }
        public int MaxSkillExperienceLevel { get => maxSkillExperienceLevel; set => maxSkillExperienceLevel = value; }
        public int RequiredSkillLevel { get => requiredSkillLevel; set => requiredSkillLevel = value; }
        public int SkillExperienceReward { get => skillExperienceReward; set => skillExperienceReward = value; }
        public int CharacterExperienceReward { get => characterExperienceReward; set => characterExperienceReward = value; }
        public float ChanceToGainLevel { get => chanceToGainLevel; set => chanceToGainLevel = value; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            craftAbility = null;
            if (craftAbilityName != null) {
                Ability baseAbility = systemDataFactory.GetResource<Ability>(craftAbilityName);
                if (baseAbility != null) {
                    craftAbility = baseAbility.AbilityProperties as CraftAbilityProperties;
                } else {
                    Debug.LogError($"Recipe.SetupScriptableObjects(): Could not find ability : {craftAbilityName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }

            if (skillName != string.Empty) {
                skill = systemDataFactory.GetResource<Skill>(skillName);
                if (skill == null) {
                    Debug.LogError($"Recipe.SetupScriptableObjects(): Could not find skill : {skillName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }

            if (holdableObjectList != null) {
                foreach (AbilityAttachmentNode holdableObjectAttachment in holdableObjectList) {
                    if (holdableObjectAttachment != null) {
                        holdableObjectAttachment.SetupScriptableObjects(ResourceName, systemGameManager);
                    }
                }
            }

            output = null;
            if (itemOutputName != null && itemOutputName != string.Empty) {
                Item item = systemDataFactory.GetResource<Item>(itemOutputName);
                if (item != null) {
                    output = item;
                } else {
                    Debug.LogError($"Recipe.SetupScriptableObjects(): Could not find item : {itemOutputName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }

            if (craftingMaterials != null) {
                foreach (CraftingMaterial craftingMaterial in craftingMaterials) {
                    craftingMaterial.SetupScriptableObjects(systemGameManager, this);
                }
            }
        }

    }

}