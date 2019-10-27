using AnyRPG;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingNode : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

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
        public override string MyInteractionPanelTitle { get => (MyAbility != null ? MyAbility.MyName : base.MyInteractionPanelTitle); }

        /// <summary>
        /// The ability to cast in order to mine this node
        /// </summary>
        [SerializeField]
        private BaseAbility ability;

        public BaseAbility MyAbility { get => ability; }

        public override bool Interact(CharacterUnit source) {
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

        public override bool SetMiniMapText(Text text) {
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
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

    }

}