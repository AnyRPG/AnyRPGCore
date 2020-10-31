using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingNodeComponent : InteractableOptionComponent {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        private CraftingNodeProps interactableOptionProps = null;

        private BaseAbility ability;

        // crafting nodes are special.  The image is based on what ability it supports
        public override Sprite Icon {
            get {
                return (MyAbility.Icon != null ? MyAbility.Icon : base.Icon);
            }
        }

        public override Sprite NamePlateImage {
            get {
                return (MyAbility.Icon != null ? MyAbility.Icon : base.NamePlateImage);
            }
        }

        public override string InteractionPanelTitle { get => (MyAbility != null ? MyAbility.DisplayName : base.InteractionPanelTitle); }
        public BaseAbility MyAbility { get => ability; }

        public CraftingNodeComponent(Interactable interactable, CraftingNodeProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
        }


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
            if (interactableOptionProps.AbilityName != null && interactableOptionProps.AbilityName != string.Empty) {
                BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(interactableOptionProps.AbilityName);
                if (baseAbility != null) {
                    ability = baseAbility;
                } else {
                    Debug.LogError("CraftingNodeComponent.SetupScriptableObjects(): COULD NOT FIND ABILITY " + interactableOptionProps.AbilityName + " while initializing ");
                }
            }
        }

    }

}