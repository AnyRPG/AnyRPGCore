using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class GatheringNodeProps : LootableNodeProps {

        [Header("Gathering Node")]

        [Tooltip("The ability to cast in order to gather from this node")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(GatherAbility))]
        private string abilityName = string.Empty;

        [Tooltip("The skill required to gather from this node, or empty for none.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Skill))]
        private string skillName = string.Empty;

        [Tooltip("The required skill level to gather from this node.")]
        [SerializeField]
        private int requiredSkillLevel = 0;

        [Tooltip("The maximum skill level at which experience will be granted for this node.  If the character skill is higher than this level, they will get no experience. 0 means this node will never stop giving experience.")]
        [SerializeField]
        private int maxSkillExperienceLevel = 0;

        [Tooltip("The maximum character level at which experience will be granted for this node.  If the character is higher than this level, they will get no experience. 0 means this node will never stop giving experience.")]
        [SerializeField]
        private int maxCharacterExperienceLevel = 0;

        private Skill skill = null;
        private GatherAbility baseAbility = null;

        // gathering nodes are special.  The image is based on what ability it supports
        public override Sprite Icon {
            get {
                return (BaseAbility.Icon != null ? BaseAbility.Icon : base.Icon);
            }
        }

        public override Sprite NamePlateImage {
            get {
                return (BaseAbility.Icon != null ? BaseAbility.Icon : base.NamePlateImage);
            }
        }

        public GatherAbility BaseAbility { get => baseAbility; }
        public int RequiredSkillLevel { get => requiredSkillLevel; set => requiredSkillLevel = value; }
        public Skill Skill { get => skill; set => skill = value; }
        public int MaxSkillExperienceLevel { get => maxSkillExperienceLevel; set => maxSkillExperienceLevel = value; }
        public int MaxCharacterExperienceLevel { get => maxCharacterExperienceLevel; set => maxCharacterExperienceLevel = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new GatheringNodeComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            if (abilityName != string.Empty) {
                GatherAbility tmpBaseAbility = systemDataFactory.GetResource<Ability>(abilityName) as GatherAbility;
                if (tmpBaseAbility != null) {
                    baseAbility = tmpBaseAbility;
                } else {
                    Debug.LogError("GatheringNode.SetupScriptableObjects(): could not find ability " + abilityName);
                }
            }
            if (skillName != string.Empty) {
                skill = systemDataFactory.GetResource<Skill>(skillName);
                if (skill == null) {
                    Debug.LogError("GatheringNode.SetupScriptableObjects(): could not find skill " + skillName);
                }
            }

        }
    }

}