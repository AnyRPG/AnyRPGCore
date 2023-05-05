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

        protected int optionIndex = 0;

        // game manager references
        protected PlayerManager playerManager = null;

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

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public void Setup(InteractableOptionComponent interactableOptionComponent, int optionIndex) {

            InteractableOption = interactableOptionComponent;
            text.text = interactableOptionComponent?.InteractableOptionProps?.GetInteractionPanelTitle(optionIndex);
            text.color = Color.white;
            this.optionIndex = optionIndex;
        }

        public override void ButtonClickAction() {
            //Debug.Log("InteractionPanelScript.ButtonClickAction()");

            base.ButtonClickAction();
            if (playerManager.UnitController != null) {
                InteractableOption.Interact(playerManager.UnitController.CharacterUnit, optionIndex);
            }
            InteractableOption.Interactable.CloseInteractionWindow();

        }

    }

}