using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class InteractionPanelScript : HighlightButton {

        [Header("Interaction Panel Option")]

        [SerializeField]
        protected Image icon = null;

        protected InteractableOptionComponent interactableOption = null;

        private int componentIndex = 0;
        private int choiceIndex = 0;

        // game manager references
        protected PlayerManager playerManager = null;
        protected InteractionManager interactionManager = null;

        public Image Icon { get => icon; set => icon = value; }
        public InteractableOptionComponent InteractableOption {
            get => interactableOption;
            set {
                if (value.InteractableOptionProps.Icon != null) {
                    icon.sprite = value.InteractableOptionProps.Icon;
                    icon.color = Color.white;
                } else {
                    icon.sprite = null;
                    icon.color = new Color32(0, 0, 0, 0);
                }
                interactableOption = value;
            }
        }

        public int ComponentIndex { get => componentIndex; }
        public int ChoiceIndex { get => choiceIndex; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            interactionManager = systemGameManager.InteractionManager;
        }

        public void Setup(InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {

            InteractableOption = interactableOptionComponent;
            text.text = interactableOptionComponent?.GetInteractionButtonText(playerManager.UnitController, componentIndex, choiceIndex);
            text.color = Color.white;
            this.componentIndex = componentIndex;
            this.choiceIndex = choiceIndex;
        }

        public override void ButtonClickAction() {
            //Debug.Log($"InteractionPanelScript.ButtonClickAction({text.text}, {componentIndex}, {choiceIndex})");

            base.ButtonClickAction();
            if (playerManager.UnitController != null) {
                interactionManager.InteractWithOptionClient(playerManager.UnitController, InteractableOption.Interactable, interactableOption, componentIndex, choiceIndex);
            }
            InteractableOption.Interactable.CloseInteractionWindow();

        }

    }

}