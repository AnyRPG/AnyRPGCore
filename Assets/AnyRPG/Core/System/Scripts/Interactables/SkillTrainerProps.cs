using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SkillTrainerProps : InteractableOptionProps {

        [Tooltip("the skills that this interactable option offers")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Skill))]
        private List<string> skillNames = new List<string>();

        private List<Skill> skills = new List<Skill>();

        public override Sprite Icon { get => (systemConfigurationManager.SkillTrainerInteractionPanelImage != null ? systemConfigurationManager.SkillTrainerInteractionPanelImage : base.Icon); }
        public override Sprite NameplateImage { get => (systemConfigurationManager.SkillTrainerNameplateImage != null ? systemConfigurationManager.SkillTrainerNameplateImage : base.NameplateImage); }

        public List<Skill> Skills { get => skills; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new SkillTrainerComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            if (skillNames != null) {
                foreach (string skillName in skillNames) {
                    Skill tmpSkill = systemDataFactory.GetResource<Skill>(skillName);
                    if (tmpSkill != null) {
                        //Debug.Log("SkillTrainerProps.SetupScriptableObjects(): Added skill : " + skillName);
                        skills.Add(tmpSkill);
                    } else {
                        Debug.LogError("SkillTrainerProps.SetupScriptableObjects(): Could not find skill : " + skillName + " while inititalizing.  CHECK INSPECTOR");
                    }
                }
            }

        }
    }

}