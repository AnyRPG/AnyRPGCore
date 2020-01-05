using AnyRPG;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class PortalInteractable : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyPortalInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyPortalInteractionPanelImage : base.MyIcon); }
        public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyPortalNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyPortalNamePlateImage : base.MyNamePlateImage); }

        /// <summary>
        /// The ability to cast in order to use this portal
        /// </summary>
        //[SerializeField]
        private BaseAbility ability;

        /// <summary>
        /// The ability to cast in order to use this portal
        /// </summary>
        [SerializeField]
        private string abilityName;


        public IAbility MyAbility { get => ability; }

        protected override void Awake() {
            //Debug.Log("Portal.Awake()");
            base.Awake();
        }

        public override bool Interact(CharacterUnit source) {
            Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
            base.Interact(source);
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact(): about to close interaction window");
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact(): window should now be closed!!!!!!!!!!!!!!!!!");
            source.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(Text text) {
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

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (abilityName != null && abilityName != string.Empty) {
                BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(abilityName);
                if (baseAbility != null) {
                    ability = baseAbility;
                } else {
                    Debug.LogError(gameObject.name + ".PortalInteractable.SetupScriptableObjects(): COULD NOT FIND ABILITY " + abilityName + " while initializing " + gameObject.name);
                }
            }
        }

    }

}