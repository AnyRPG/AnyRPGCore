using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class LootableNodeComponent : InteractableOptionComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public LootableNodeProps Props { get => interactableOptionProps as LootableNodeProps; }

        protected float currentTimer = 0f;

        protected bool lootDropped = false;

        // track the number of times this item has been picked up
        protected int pickupCount = 0;

        protected Coroutine spawnCoroutine = null;

        public override bool MyPrerequisitesMet {
            get {
                bool returnResult = base.MyPrerequisitesMet;
                if (returnResult == false) {
                    return returnResult;
                }
                if (spawnCoroutine != null) {
                    return false;
                }
                return returnResult;
            }
        }

        public LootableNodeComponent(Interactable interactable, LootableNodeProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }

        public override void Cleanup() {
            base.Cleanup();
            ClearLootTables();
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".LootableNode.Interact(" + source.name + ")");
            if (Props.LootTables == null) {
                //Debug.Log(gameObject.name + ".GatheringNode.Interact(" + source.name + "): lootTable was null!");
                return true;
            }
            base.Interact(source, optionIndex);

            DropLoot();
            PickUp();
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            return true;
        }

        protected IEnumerator StartSpawnCountdown() {
            //Debug.Log(gameObject.name + ".LootableNode.StartSpawnCountdown()");

            // DISABLE MINIMAP ICON WHILE ITEM IS NOT SPAWNED
            HandlePrerequisiteUpdates();

            currentTimer = Props.SpawnTimer;
            while (currentTimer > 0) {
                //Debug.Log("Spawn Timer: " + currentTimer);
                yield return new WaitForSeconds(1);
                currentTimer -= 1;
            }
            spawnCoroutine = null;

            //Debug.Log(gameObject.name + ".LootableNode.StartSpawnCountdown(): countdown complete");
            //interactable.Spawn();

            // ENABLE MINIMAP ICON AFTER SPAWN
            HandlePrerequisiteUpdates();
        }


        public virtual void DropLoot() {
            //Debug.Log(gameObject.name + ".LootableNode.DropLoot()");

            // is the below code necessary?  it was causing stuff that was already dropped but not picked up to not pop a window again and just remain unlootable
            /*
            if (lootDropped) {
                // add this to prevent double drops from child classes like GatheringNode
                return;
            }
            */

            List<LootDrop> lootDrops = new List<LootDrop>();
            foreach (LootTable lootTable in Props.LootTables) {
                lootDrops.AddRange(lootTable.GetLoot());
            }
            LootUI.MyInstance.CreatePages(lootDrops);
            lootDropped = true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>
        public void PickUp() {
            //Debug.Log(gameObject.name + ".LootableNode.Pickup()");
            //LootUI.MyInstance.CreatePages(lootTable.GetLoot());
            CreateWindowEventSubscriptions();
            LootUI.MyInstance.OnCloseWindow += ClearTakeLootHandler;
            PopupWindowManager.MyInstance.lootWindow.OpenWindow();
        }

        public void ClearTakeLootHandler(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".LootableNode.ClearTakeLootHandler()");
            CleanupWindowEventSubscriptions();
        }

        public void CreateWindowEventSubscriptions() {
            //Debug.Log(gameObject.name + ".LootableNode.CreateWindowEventSubscriptions()");
            SystemEventManager.MyInstance.OnTakeLoot += CheckDropListSize;
        }

        public void CleanupWindowEventSubscriptions() {
            //Debug.Log(gameObject.name + ".LootableNode.CleanupWindowEventSubscriptions()");
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnTakeLoot -= CheckDropListSize;
            }
            if (LootUI.MyInstance != null) {
                LootUI.MyInstance.OnCloseWindow -= ClearTakeLootHandler;
            }
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log("GatheringNode.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public void ClearLootTables() {
            Props.LootTables.Clear();
        }

        public void CheckDropListSize() {
            //Debug.Log(gameObject.name + ".LootableNode.CheckDropListSize()");
            int lootCount = 0;
            foreach (LootTable lootTable in Props.LootTables) {
                lootCount += lootTable.MyDroppedItems.Count;
            }
            if (lootCount == 0) {
                // since this method is only called on take loot, we can consider everything picked up if there is no loot left
                pickupCount++;

                lootDropped = false;
                //if (lootTable.MyDroppedItems.Count == 0) {
                PlayerManager.MyInstance.PlayerController.RemoveInteractable(interactable);
                interactable.DestroySpawn();
                foreach (LootTable lootTable in Props.LootTables) {
                    lootTable.Reset();
                }

                // spawn timer of -1 means don't spawn again
                if (spawnCoroutine == null && Props.SpawnTimer >= 0f) {
                    //Debug.Log(gameObject.name + ".LootableNode.CheckDropListSize(): starting countdown; spawnTimer: " + spawnTimer);
                    spawnCoroutine = interactable.StartCoroutine(StartSpawnCountdown());
                }

                // loot being gone is a type of prerequisite for a lootable node
                HandlePrerequisiteUpdates();
            }
        }

        public override void StopInteract() {
            base.StopInteract();

            PopupWindowManager.MyInstance.lootWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log(interactable.gameObject.name + ".LootableNode.SetMiniMapText()");
            if (!base.SetMiniMapText(text)) {
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            text.text = "o";
            text.color = Color.blue;
            return true;
        }

        /*
        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".GatheringNode.GetCurrentOptionCount()");
            return (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(MyAbility.MyName) == true && interactable.MySpawnReference != null ? 1 : 0);
        }
        */

        public override void CallMiniMapStatusUpdateHandler() {
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".LootableNode.HandlePlayerUnitSpawn()");
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false, float factionValue = 0f, bool processNonCombatCheck = true) {
            //Debug.Log(interactable.gameObject.name + ".LootableNode.CanInteract()");
            bool returnValue = base.CanInteract(processRangeCheck, passedRangeCheck, factionValue, processNonCombatCheck);
            if (returnValue == false) {
                return false;
            }
            if (spawnCoroutine != null) {
                return false;
            }
            return (GetCurrentOptionCount() == 0 ? false : true);
        }
    }

}