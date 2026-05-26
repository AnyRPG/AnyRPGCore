using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SpecializationChangeProps : InteractableOptionProps {

        [Tooltip("the class Specialization that this interactable option offers")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ClassSpecialization))]
        private string specializationName = string.Empty;

        private ClassSpecialization classSpecialization;

        public override Sprite Icon { get => (systemConfigurationManager.ClassChangeInteractionPanelImage != null ? systemConfigurationManager.ClassChangeInteractionPanelImage : base.Icon); }
        public override Sprite NameplateImage { get => (systemConfigurationManager.ClassChangeNameplateImage != null ? systemConfigurationManager.ClassChangeNameplateImage : base.NameplateImage); }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new SpecializationChangeComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (classSpecialization == null && specializationName != null && specializationName != string.Empty) {
                ClassSpecialization tmpClassSpecialization = systemDataFactory.GetResource<ClassSpecialization>(specializationName);
                if (tmpClassSpecialization != null) {
                    classSpecialization = tmpClassSpecialization;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find specialization : " + specializationName + " while inititalizing.  CHECK INSPECTOR");
                }

            }

        }
    }

}