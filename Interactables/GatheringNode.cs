using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class GatheringNode : LootableNode {

        //public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private GatheringNodeConfig gatheringNodeConfig = new GatheringNodeConfig();

        // gathering nodes are special.  The image is based on what ability it supports
        public override Sprite Icon {
            get {
                return (BaseAbility.Icon != null ? BaseAbility.Icon : base.Icon);
            }
        }

        public override Sprite NamePlateImage {
            get {
                return (BaseAbility.Icon != null ? BaseAbility.Icon : base.NamePlateImage);
            }
        }
        public override string InteractionPanelTitle { get => (BaseAbility != null ? BaseAbility.DisplayName : base.InteractionPanelTitle); }

        /// <summary>
        /// The ability to cast in order to mine this node
        /// </summary>
        [SerializeField]
        private string abilityName = string.Empty;

        private GatherAbility baseAbility = null;

        public GatherAbility BaseAbility { get => baseAbility; }

        public GatheringNode(Interactable interactable, GatheringNodeConfig interactableOptionConfig) : base(interactable, interactableOptionConfig) {
            this.gatheringNodeConfig = interactableOptionConfig;
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


        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".GatheringNode.Interact(" + source.name + ")");
            if (lootTableNames == null) {
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
                Gather();
            } else {
                source.GetComponent<CharacterUnit>().BaseCharacter.CharacterAbilityManager.BeginAbility(BaseAbility, gameObject);
            }
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            return true;
            //return PickUp();
        }

        public void Gather() {
            //Debug.Log(gameObject.name + ".GatheringNode.DropLoot()");
            base.Interact(PlayerManager.MyInstance.MyCharacter.CharacterUnit);

        }

        /*
        public override void DropLoot() {
            Debug.Log(gameObject.name + ".GatheringNode.DropLoot()");
            base.Interact(PlayerManager.MyInstance.MyCharacter.CharacterUnit);
            //base.DropLoot();
            //PickUp();
        }
        */

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".GatheringNode.GetCurrentOptionCount()");
            return ((PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.HasAbility(BaseAbility) == true && interactable.MySpawnReference != null) ? 1 : 0);
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

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (abilityName != null && abilityName != string.Empty) {
                GatherAbility tmpBaseAbility = SystemAbilityManager.MyInstance.GetResource(abilityName) as GatherAbility;
                if (tmpBaseAbility != null) {
                    baseAbility = tmpBaseAbility;
                } else {
                    Debug.LogError(gameObject.name + ".GatheringNode.SetupScriptableObjects(): could not find ability " + abilityName);
                }
            }

        }
    }

}