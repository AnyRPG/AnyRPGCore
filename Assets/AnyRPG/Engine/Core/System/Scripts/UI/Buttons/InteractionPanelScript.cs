using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class InteractionPanelScript : MonoBehaviour {


        [SerializeField]
        private TextMeshProUGUI text = null;

        [SerializeField]
        private Image icon = null;

        private InteractableOptionComponent interactableOption = null;

        private int optionIndex = 0;

        public Image MyIcon { get => icon; set => icon = value; }
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

        public void Setup(InteractableOptionComponent interactableOptionComponent, int optionIndex) {

            InteractableOption = interactableOptionComponent;
            text.text = interactableOptionComponent?.InteractableOptionProps?.GetInteractionPanelTitle(optionIndex);
            text.color = Color.white;
            this.optionIndex = optionIndex;
        }

        public void Interact() {
            if (PlayerManager.Instance.UnitController != null) {
                InteractableOption.Interact(PlayerManager.Instance.UnitController.CharacterUnit, optionIndex);
            }
            InteractableOption.Interactable.CloseInteractionWindow();
        }

    }

}