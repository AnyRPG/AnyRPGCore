using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingNodeComponent : InteractableOptionComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public CraftingNodeProps Props { get => interactableOptionProps as CraftingNodeProps; }

        public CraftingNodeComponent(Interactable interactable, CraftingNodeProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }

        public override void CreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();

            // because the skill is a special type of prerequisite, we need to be notified when it changes
            if (SystemEventManager.MyInstance == null) {
                Debug.LogError("SystemEventManager Not Found.  Is the GameManager prefab in the scene?");
                return;
            }
            SystemEventManager.MyInstance.OnAbilityListChanged += HandleAbilityListChange;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log("GatheringNode.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();

            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnAbilityListChanged -= HandleAbilityListChange;
            }
        }

        public void HandleAbilityListChange(BaseAbility baseAbility) {
            //Debug.Log(gameObject.name + ".GatheringNode.HandleAbilityListChange(" + baseAbility.DisplayName + ")");
            HandlePrerequisiteUpdates();
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".GatheringNode.GetCurrentOptionCount()");
            return ((PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.HasAbility(Props.Ability) == true) ? 1 : 0);
        }

        public override bool Interact(CharacterUnit source) {
            base.Interact(source);

            CraftingUI.MyInstance.ViewRecipes(Props.Ability as CraftAbility);
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

    }

}