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

        public TextMeshProUGUI MyText {
            get {
                return text;
            }
        }

        private InteractableOptionComponent interactableOption;

        public Image MyIcon { get => icon; set => icon = value; }
        public InteractableOptionComponent MyInteractableOption {
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

        public void Interact() {
            if (PlayerManager.MyInstance.UnitController != null) {
                MyInteractableOption.Interact(PlayerManager.MyInstance.UnitController.CharacterUnit);
            }
            MyInteractableOption.Interactable.CloseInteractionWindow();
        }

    }

}