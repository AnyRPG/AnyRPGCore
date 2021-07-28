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

        public CraftingNodeProps Props { get => interactableOptionProps as CraftingNodeProps; }

        public CraftingNodeComponent(Interactable interactable, CraftingNodeProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }

        public override bool MyPrerequisitesMet {
            get {
                if (PlayerManager.Instance.MyCharacter.CharacterAbilityManager.HasAbility(Props.Ability) == false) {
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

            // because the skill is a special type of prerequisite, we need to be notified when it changes
            if (SystemGameManager.Instance.EventManager == null) {
                Debug.LogError("SystemEventManager Not Found.  Is the GameManager prefab in the scene?");
                return;
            }
            SystemGameManager.Instance.EventManager.OnAbilityListChanged += HandleAbilityListChange;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log("GatheringNode.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();

            if (SystemGameManager.Instance.EventManager != null) {
                SystemGameManager.Instance.EventManager.OnAbilityListChanged -= HandleAbilityListChange;
            }
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
            return ((PlayerManager.Instance.MyCharacter.CharacterAbilityManager.HasAbility(Props.Ability) == true) ? 1 : 0);
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            base.Interact(source, optionIndex);

            if (Props == null || Props.Ability == null) {
                Debug.Log("Props is null");
            }
            SystemGameManager.Instance.CraftingManager.SetAbility(Props.Ability as CraftAbility);
            //source.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
            return true;
            //return PickUp();
        }

        public override void StopInteract() {
            base.StopInteract();

            PopupWindowManager.Instance.craftingWindow.CloseWindow();
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