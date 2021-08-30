using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingNodeComponent : InteractableOptionComponent {

        // game manager references
        private CraftingManager craftingManager = null;

        public CraftingNodeProps Props { get => interactableOptionProps as CraftingNodeProps; }

        public CraftingNodeComponent(Interactable interactable, CraftingNodeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            craftingManager = systemGameManager.CraftingManager;
        }

        public override bool MyPrerequisitesMet {
            get {
                if (playerManager.MyCharacter.CharacterAbilityManager.HasAbility(Props.Ability) == false) {
                    return false;
                }
                return base.MyPrerequisitesMet;
            }
        } 

        public override void CreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();

            systemEventManager.OnAbilityListChanged += HandleAbilityListChange;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log("GatheringNode.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();

            systemEventManager.OnAbilityListChanged -= HandleAbilityListChange;
        }

        public static List<CraftingNodeComponent> GetCraftingNodeComponents(Interactable searchInteractable) {
            if (searchInteractable == null) {
                return new List<CraftingNodeComponent>();
            }
            return searchInteractable.GetInteractableOptionList(typeof(CraftingNodeComponent)).Cast<CraftingNodeComponent>().ToList();
        }

        public void HandleAbilityListChange(BaseAbility baseAbility) {
            //Debug.Log(gameObject.name + ".GatheringNode.HandleAbilityListChange(" + baseAbility.DisplayName + ")");
            HandlePrerequisiteUpdates();
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".GatheringNode.GetCurrentOptionCount()");
            return ((playerManager.MyCharacter.CharacterAbilityManager.HasAbility(Props.Ability) == true) ? 1 : 0);
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            base.Interact(source, optionIndex);

            if (Props == null || Props.Ability == null) {
                Debug.Log("Props is null");
            }
            craftingManager.SetAbility(Props.Ability as CraftAbility);
            //source.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
            return true;
            //return PickUp();
        }

        public override void StopInteract() {
            base.StopInteract();

            uIManager.craftingWindow.CloseWindow();
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
            text.color = Color.blue;
            return true;
        }

    }

}