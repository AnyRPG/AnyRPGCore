using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingNode : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        /// <summary>
        /// The ability to cast in order to mine this node
        /// </summary>
        //[SerializeField]
        private BaseAbility ability;

        /// <summary>
        /// The ability to cast in order to mine this node
        /// </summary>
        [SerializeField]
        private string abilityName = string.Empty;

        // crafting nodes are special.  The image is based on what ability it supports
        public override Sprite MyIcon {
            get {
                return (MyAbility.MyIcon != null ? MyAbility.MyIcon : base.MyIcon);
            }
        }

        public override Sprite MyNamePlateImage {
            get {
                return (MyAbility.MyIcon != null ? MyAbility.MyIcon : base.MyNamePlateImage);
            }
        }

        public override string InteractionPanelTitle { get => (MyAbility != null ? MyAbility.DisplayName : base.InteractionPanelTitle); }
        public BaseAbility MyAbility { get => ability; }

        public override bool Interact(CharacterUnit source) {
            base.Interact(source);

            CraftingUI.MyInstance.ViewRecipes(ability as CraftAbility);
            //source.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
            return true;
            //return PickUp();
        }

        public override void StopInteract() {
            base.StopInteract();

            PopupWindowManager.MyInstance.craftingWindow.CloseWindow();
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
            text.color = Color.blue;
            return true;
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + "CraftingNode.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


        public override void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + "CraftingNode.SetupScriptableObjects()");
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