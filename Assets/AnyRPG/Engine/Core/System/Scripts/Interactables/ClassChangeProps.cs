using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class ClassChangeProps : InteractableOptionProps {

        [Tooltip("the class that this interactable option offers")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(CharacterClass))]
        private string className = string.Empty;

        private CharacterClass characterClass;

        public override Sprite Icon { get => (SystemGameManager.Instance.SystemConfigurationManager.ClassChangeInteractionPanelImage != null ? SystemGameManager.Instance.SystemConfigurationManager.ClassChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemGameManager.Instance.SystemConfigurationManager.ClassChangeNamePlateImage != null ? SystemGameManager.Instance.SystemConfigurationManager.ClassChangeNamePlateImage : base.NamePlateImage); }

        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new ClassChangeComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (className != null && className != string.Empty) {
                CharacterClass tmpCharacterClass = SystemCharacterClassManager.Instance.GetResource(className);
                if (tmpCharacterClass != null) {
                    characterClass = tmpCharacterClass;
                } else {
                    Debug.LogError("ClassChangeComponent.SetupScriptableObjects(): Could not find faction : " + className + " while inititalizing.  CHECK INSPECTOR");
                }

            }

        }
    }

}