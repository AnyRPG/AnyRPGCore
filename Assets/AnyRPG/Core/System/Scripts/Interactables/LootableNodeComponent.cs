using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class LootableNodeComponent : InteractableOptionComponent, ILootHolder {

        public LootableNodeProps Props { get => interactableOptionProps as LootableNodeProps; }

        protected float currentTimer = 0f;

        protected bool lootDropped = false;

        // track the number of times this item has been picked up
        protected int pickupCount = 0;

        protected LootHolder lootHolder = new LootHolder();

        protected Coroutine spawnCoroutine = null;

        // game manager references
        protected LootManager lootManager = null;

        public override bool PrerequisitesMet {
            get {
                bool returnResult = base.PrerequisitesMet;
                if (returnResult == false) {
                    return returnResult;
                }
                if (spawnCoroutine != null) {
                    return false;
                }
                if (Props.SpawnTimer == -1 && pickupCount > 0) {
                    return false;
                }
                return returnResult;
            }
        }

        public LootHolder LootHolder { get => lootHolder; set => lootHolder = value; }

        public LootableNodeComponent(Interactable interactable, LootableNodeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            // initialize loot tables and states
            InitializeLootTableStates();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            lootManager = systemGameManager.LootManager;
        }

        public override void Cleanup() {
            base.Cleanup();
            ClearLootTables();
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log($"{gameObject.name}.LootableNode.Interact(" + source.name + ")");
            if (Props.LootTables == null) {
                //Debug.Log($"{gameObject.name}.GatheringNode.Interact(" + source.name + "): lootTable was null!");
                return true;
            }
            base.Interact(source, optionIndex);

            DropLoot();
            PickUp();
            uIManager.interactionWindow.CloseWindow();
            return true;
        }

        protected IEnumerator StartSpawnCountdown() {
            //Debug.Log(interactable.gameObject.name + ".LootableNode.StartSpawnCountdown()");

            // DISABLE MINIMAP ICON WHILE ITEM IS NOT SPAWNED
            // this next line is already done outside the loop so should not be needed here ?
            //HandlePrerequisiteUpdates();

            currentTimer = Props.SpawnTimer;
            while (currentTimer > 0) {
                //Debug.Log("Spawn Timer: " + currentTimer);
                yield return new WaitForSeconds(1);
                currentTimer -= 1;
            }
            spawnCoroutine = null;

            //Debug.Log($"{gameObject.name}.LootableNode.StartSpawnCountdown(): countdown complete");
            //interactable.Spawn();

            // ENABLE MINIMAP ICON AFTER SPAWN
            HandlePrerequisiteUpdates();
        }


        public virtual void DropLoot() {
            //Debug.Log($"{gameObject.name}.LootableNode.DropLoot()");

            // is the below code necessary?  it was causing stuff that was already dropped but not picked up to not pop a window again and just remain unlootable
            /*
            if (lootDropped) {
                // add this to prevent double drops from child classes like GatheringNode
                return;
            }
            */

            List<LootDrop> lootDrops = new List<LootDrop>();
            foreach (LootTable lootTable in Props.LootTables) {
                lootDrops.AddRange(lootHolder.LootTableStates[lootTable].GetLoot(lootTable));
            }
            //lootManager.CreatePages(lootDrops);
            lootManager.AddLoot(lootDrops);
            lootDropped = true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>
        public void PickUp() {
            //Debug.Log($"{gameObject.name}.LootableNode.Pickup()");
            CreateWindowEventSubscriptions();
            uIManager.lootWindow.CloseableWindowContents.OnCloseWindow += ClearTakeLootHandler;
            uIManager.lootWindow.OpenWindow();
        }

        //public void ClearTakeLootHandler(ICloseableWindowContents windowContents) {
        public void ClearTakeLootHandler(CloseableWindowContents windowContents) {
            //Debug.Log($"{gameObject.name}.LootableNode.ClearTakeLootHandler()");
            CleanupWindowEventSubscriptions();
        }

        public void CreateWindowEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.LootableNode.CreateWindowEventSubscriptions()");
            SystemEventManager.StartListening("OnTakeLoot", HandleTakeLoot);
        }

        public void CleanupWindowEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.LootableNode.CleanupWindowEventSubscriptions()");
            SystemEventManager.StopListening("OnTakeLoot", HandleTakeLoot);
            if (uIManager?.lootWindow?.CloseableWindowContents != null) {
                uIManager.lootWindow.CloseableWindowContents.OnCloseWindow -= ClearTakeLootHandler;
            }
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("GatheringNode.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public void HandleTakeLoot(string eventName, EventParamProperties eventParamProperties) {
            CheckDropListSize();
        }

        public void ClearLootTables() {
            Props.LootTables.Clear();
        }

        public void CheckDropListSize() {
            //Debug.Log($"{gameObject.name}.LootableNode.CheckDropListSize()");
            int lootCount = 0;
            foreach (LootTable lootTable in Props.LootTables) {
                lootCount += lootHolder.LootTableStates[lootTable].DroppedItems.Count;
            }
            if (lootCount == 0) {
                // since this method is only called on take loot, we can consider everything picked up if there is no loot left
                pickupCount++;

                lootDropped = false;
                //if (lootTable.MyDroppedItems.Count == 0) {
                // TODO : monitor is this next line needed if the interactable will handle a generic status update?
                //playerManager.PlayerController.RemoveInteractable(interactable);


                // testing : monitor if this affects pickup nodes.  Theoretically the HandlePrerequisiteUpdates() call should trigger a despawn anyway
                //interactable.DestroySpawn();

                InitializeLootTableStates();

                // spawn timer of -1 means don't spawn again
                if (spawnCoroutine == null && Props.SpawnTimer >= 0f) {
                    //Debug.Log($"{gameObject.name}.LootableNode.CheckDropListSize(): starting countdown; spawnTimer: " + spawnTimer);
                    spawnCoroutine = interactable.StartCoroutine(StartSpawnCountdown());
                }

                // loot being gone is a type of prerequisite for a lootable node
                // DISABLE MINIMAP ICON WHILE ITEM IS NOT SPAWNED

                HandlePrerequisiteUpdates();
            }
        }

        private void InitializeLootTableStates() {
            lootHolder.InitializeLootTableStates();
            foreach (LootTable lootTable in Props.LootTables) {
                lootHolder.AddLootTableState(lootTable, new LootTableState(systemGameManager));
            }
        }

        public override void StopInteract() {
            base.StopInteract();

            uIManager.lootWindow.CloseWindow();
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