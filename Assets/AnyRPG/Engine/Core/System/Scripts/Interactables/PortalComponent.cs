using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class PortalComponent : InteractableOptionComponent {

        public PortalProps Props { get => interactableOptionProps as PortalProps; }

        public PortalComponent(Interactable interactable, PortalProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }
        

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
            base.Interact(source, optionIndex);
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact(): about to close interaction window");
            SystemGameManager.Instance.UIManager.PopupWindowManager.interactionWindow.CloseWindow();
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact(): window should now be closed!!!!!!!!!!!!!!!!!");
            if (Props.LocationTag != null && Props.LocationTag != string.Empty) {
                LevelManager.Instance.OverrideSpawnLocationTag = Props.LocationTag;
            }
            return true;
        }

        public override void StopInteract() {
            base.StopInteract();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            if (!base.SetMiniMapText(text)) {
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            text.text = "o";
            text.color = Color.cyan;
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".PortalInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

    }

}