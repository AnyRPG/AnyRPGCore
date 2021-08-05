using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class SpecializationChangeProps : InteractableOptionProps {

        [Tooltip("the class Specialization that this interactable option offers")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ClassSpecialization))]
        private string specializationName = string.Empty;

        private ClassSpecialization classSpecialization;

        public override Sprite Icon { get => (SystemGameManager.Instance.SystemConfigurationManager.ClassChangeInteractionPanelImage != null ? SystemGameManager.Instance.SystemConfigurationManager.ClassChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemGameManager.Instance.SystemConfigurationManager.ClassChangeNamePlateImage != null ? SystemGameManager.Instance.SystemConfigurationManager.ClassChangeNamePlateImage : base.NamePlateImage); }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new SpecializationChangeComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (classSpecialization == null && specializationName != null && specializationName != string.Empty) {
                ClassSpecialization tmpClassSpecialization = SystemDataFactory.Instance.GetResource<ClassSpecialization>(specializationName);
                if (tmpClassSpecialization != null) {
                    classSpecialization = tmpClassSpecialization;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find specialization : " + specializationName + " while inititalizing.  CHECK INSPECTOR");
                }

            }

        }
    }

}