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
        private string specializationName = string.Empty;

        private ClassSpecialization classSpecialization;

        public override Sprite Icon { get => (SystemConfigurationManager.Instance.ClassChangeInteractionPanelImage != null ? SystemConfigurationManager.Instance.ClassChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.Instance.ClassChangeNamePlateImage != null ? SystemConfigurationManager.Instance.ClassChangeNamePlateImage : base.NamePlateImage); }
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
                ClassSpecialization tmpClassSpecialization = SystemClassSpecializationManager.Instance.GetResource(specializationName);
                if (tmpClassSpecialization != null) {
                    classSpecialization = tmpClassSpecialization;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find specialization : " + specializationName + " while inititalizing.  CHECK INSPECTOR");
                }

            }

        }
    }

}