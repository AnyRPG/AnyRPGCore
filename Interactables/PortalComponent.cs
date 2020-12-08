using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class PortalComponent : InteractableOptionComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public PortalProps Props { get => interactableOptionProps as PortalProps; }

        public PortalComponent(Interactable interactable, PortalProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }
        

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
            base.Interact(source);
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact(): about to close interaction window");
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact(): window should now be closed!!!!!!!!!!!!!!!!!");
            if (Props.LocationTag != null && Props.LocationTag != string.Empty) {
                LevelManager.MyInstance.OverrideSpawnLocationTag = Props.LocationTag;
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
            text.fontSize = 50;
            text.color = Color.cyan;
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".PortalInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

        public override void HandlePrerequisiteUpdates() {
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


    }
}