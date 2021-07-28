using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class GatheringNodeComponent : LootableNodeComponent {

        public GatheringNodeProps GatheringNodeProps { get => interactableOptionProps as GatheringNodeProps; }

        public GatheringNodeComponent(Interactable interactable, GatheringNodeProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }

        public override bool MyPrerequisitesMet {
            get {
                if (PlayerManager.Instance.MyCharacter.CharacterAbilityManager.HasAbility(GatheringNodeProps.BaseAbility) == false) {
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

        public void HandleAbilityListChange(BaseAbility baseAbility) {
            //Debug.Log(gameObject.name + ".GatheringNode.HandleAbilityListChange(" + baseAbility.DisplayName + ")");
            HandlePrerequisiteUpdates();
        }

        public static GatheringNodeComponent GetGatheringNodeComponent(Interactable searchInteractable) {
            if (searchInteractable == null) {
                return null;
            }
            return searchInteractable.GetFirstInteractableOption(typeof(GatheringNodeComponent)) as GatheringNodeComponent;
        }


        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".GatheringNode.Interact(" + source.name + ")");
            if (Props.LootTables == null) {
                //Debug.Log(gameObject.name + ".GatheringNode.Interact(" + source.name + "): lootTable was null!");
                return true;
            }
            // base.Interact() will drop loot automatically so we will intentionally not call it because the loot drop in this class is activated by the gatherability
            /*
            int lootCount = 0;
            base.Interact(source);

            foreach (LootTable lootTable in lootTables) {
                if (lootTable.MyDroppedItems.Count > 0) {
                    lootCount += lootTable.MyDroppedItems.Count;
                }
            }
            */
            //if (lootCount > 0) {
            if (lootDropped == true) {
                // this call is safe, it will internally check if loot is already dropped and just pickup instead
                Gather(optionIndex);
            } else {
                source.BaseCharacter.CharacterAbilityManager.BeginAbility(GatheringNodeProps.BaseAbility, interactable);
            }
            PopupWindowManager.Instance.interactionWindow.CloseWindow();
            return true;
            //return PickUp();
        }

        public void Gather(int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".GatheringNode.DropLoot()");
            if (PlayerManager.Instance.ActiveUnitController != null) {
                base.Interact(PlayerManager.Instance.ActiveUnitController.CharacterUnit, optionIndex);
            }
        }

        /*
        public override void DropLoot() {
            Debug.Log(gameObject.name + ".GatheringNode.DropLoot()");
            base.Interact(PlayerManager.Instance.MyCharacter.CharacterUnit);
            //base.DropLoot();
            //PickUp();
        }
        */

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".GatheringNode.GetCurrentOptionCount()");
            return ((PlayerManager.Instance.MyCharacter.CharacterAbilityManager.HasAbility(GatheringNodeProps.BaseAbility) == true
                && interactable.MySpawnReference != null
                && currentTimer <= 0f) ? 1 : 0);
        }

        /*

        public override bool CanInteract(CharacterUnit source) {
            bool returnValue = base.CanInteract(source);
            if (returnValue == false) {
                return false;
            }
            return (GetCurrentOptionCount() == 0 ? false : true);
        }
        */

    }

}