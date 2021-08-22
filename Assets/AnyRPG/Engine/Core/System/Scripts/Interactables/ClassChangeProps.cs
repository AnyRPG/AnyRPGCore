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

        public override Sprite Icon { get => (systemConfigurationManager.ClassChangeInteractionPanelImage != null ? systemConfigurationManager.ClassChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.ClassChangeNamePlateImage != null ? systemConfigurationManager.ClassChangeNamePlateImage : base.NamePlateImage); }

        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new ClassChangeComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (className != null && className != string.Empty) {
                CharacterClass tmpCharacterClass = systemDataFactory.GetResource<CharacterClass>(className);
                if (tmpCharacterClass != null) {
                    characterClass = tmpCharacterClass;
                } else {
                    Debug.LogError("ClassChangeComponent.SetupScriptableObjects(): Could not find character class : " + className + " while inititalizing.  CHECK INSPECTOR");
                }

            }

        }
    }

}