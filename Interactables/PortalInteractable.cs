using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class PortalInteractable : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private PortalConfig portalConfig = new PortalConfig();

        public override Sprite Icon { get => portalConfig.Icon; }
        public override Sprite NamePlateImage { get => portalConfig.NamePlateImage; }

        [Header("Location Override")]

        [Tooltip("If this is set, the player will spawn at the location of the object in the scene with this tag, instead of the default spawn location for the scene.")]
        [SerializeField]
        protected string locationTag = string.Empty;

        public PortalInteractable(Interactable interactable, PortalConfig interactableOptionConfig) : base(interactable) {
            this.portalConfig = interactableOptionConfig;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
            base.Interact(source);
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact(): about to close interaction window");
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact(): window should now be closed!!!!!!!!!!!!!!!!!");
            if (locationTag != null && locationTag != string.Empty) {
                LevelManager.MyInstance.OverrideSpawnLocationTag = locationTag;
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