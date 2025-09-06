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

        public override bool BlockTooltip {
            get {
                if (Props.SpawnObject == null) {
                    return false;
                }
                return (Props.SpawnObject.activeSelf == false);
            }
        }

        public override bool PrerequisitesMet(UnitController sourceUnitController) {
            bool returnResult = base.PrerequisitesMet(sourceUnitController);
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

        public LootHolder LootHolder { get => lootHolder; set => lootHolder = value; }

        public LootableNodeComponent(Interactable interactable, LootableNodeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            // initialize loot tables and states
            // why was this commented out?
            InitializeLootTableStates();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            lootManager = systemGameManager.LootManager;
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            lootHolder.Configure(systemGameManager);
            SubscribeToLootHolderEvents();
        }

        public void SubscribeToLootHolderEvents() {
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                lootHolder.OnRemoveDroppedItem += HandleRemoveDroppedItem;
            }
        }

        private void HandleRemoveDroppedItem(LootDrop drop, int accountId) {
            CheckDropListSize();
        }

        public override void Cleanup() {
            base.Cleanup();
            ClearLootTables();
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{gameObject.name}.LootableNode.Interact(" + source.name + ")");
            if (Props.LootTables == null) {
                //Debug.Log($"{gameObject.name}.GatheringNode.Interact(" + source.name + "): lootTable was null!");
                return true;
            }
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            DropLoot(sourceUnitController);
            //PickUp();
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.LootableNodeComponent.ClientInteraction({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            uIManager.interactionWindow.CloseWindow();
            OpenLootWindow();
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
            Spawn();

            // ENABLE MINIMAP ICON AFTER SPAWN
            HandleOptionStateChange();
        }

        private void Spawn() {
            if (Props.SpawnObject != null && Props.SpawnObject.activeSelf == false) {
                Props.SpawnObject.SetActive(true);
            }
        }

        public virtual void DropLoot(UnitController sourceUnitController) {
            //Debug.Log($"{interactable.gameObject.name}.LootableNode.DropLoot()");

            // is the below code necessary?  it was causing stuff that was already dropped but not picked up to not pop a window again and just remain unlootable
            /*
            if (lootDropped) {
                // add this to prevent double drops from child classes like GatheringNode
                return;
            }
            */

            List<LootDrop> lootDrops = new List<LootDrop>();
            foreach (LootTable lootTable in Props.LootTables) {
                lootDrops.AddRange(lootHolder.GetLoot(sourceUnitController, lootTable, true));
            }
            //lootManager.CreatePages(lootDrops);
            lootManager.AddAvailableLoot(sourceUnitController, lootDrops);
            lootDropped = true;
        }

        public void OpenLootWindow() {
            //Debug.Log($"{interactable.gameObject.name}.LootableNodeComponent.OpenLootWindow()");

            //CreateWindowEventSubscriptions();
            //uIManager.lootWindow.CloseableWindowContents.OnCloseWindow += ClearTakeLootHandler;
            uIManager.lootWindow.OpenWindow();
        }

        /*
        //public void ClearTakeLootHandler(ICloseableWindowContents windowContents) {
        public void ClearTakeLootHandler(CloseableWindowContents windowContents) {
            //Debug.Log($"{gameObject.name}.LootableNode.ClearTakeLootHandler()");
            CleanupWindowEventSubscriptions();
        }

        public void CreateWindowEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.LootableNode.CreateWindowEventSubscriptions()");
            systemEventManager.OnTakeLoot += HandleTakeLoot;
        }

        public void CleanupWindowEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.LootableNode.CleanupWindowEventSubscriptions()");
            systemEventManager.OnTakeLoot -= HandleTakeLoot;
            if (uIManager?.lootWindow?.CloseableWindowContents != null) {
                uIManager.lootWindow.CloseableWindowContents.OnCloseWindow -= ClearTakeLootHandler;
            }
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("GatheringNode.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public void HandleTakeLoot(int accountId) {
            //Debug.Log($"{interactable.gameObject.name}.LootableNode.HandleTakeLoot({accountId})");

            CheckDropListSize();
        }
        */

        public void ClearLootTables() {
            Props.LootTables.Clear();
        }

        public void CheckDropListSize() {
            //Debug.Log($"{interactable.gameObject.name}.LootableNode.CheckDropListSize()");

            int lootCount = 0;
            foreach (Dictionary<int, LootTableState> lootTableStateDict in lootHolder.LootTableStates.Values) {
                foreach (LootTableState lootTableState in lootTableStateDict.Values) {
                    lootCount += lootTableState.DroppedItems.Count;
                }
            }
            if (lootCount == 0) {
                // since this method is only called on take loot, we can consider everything picked up if there is no loot left
                pickupCount++;

                lootDropped = false;
                //if (lootTable.MyDroppedItems.Count == 0) {
                // TODO : monitor is this next line needed if the interactable will handle a generic status update?
                //playerManager.PlayerController.RemoveInteractable(interactable);


                // testing : monitor if this affects pickup nodes.  Theoretically the HandlePrerequisiteUpdates() call should trigger a despawn anyway
                Despawn();

                InitializeLootTableStates();

                // spawn timer of -1 means don't spawn again
                if (spawnCoroutine == null && Props.SpawnTimer >= 0f) {
                    //Debug.Log($"{gameObject.name}.LootableNode.CheckDropListSize(): starting countdown; spawnTimer: " + spawnTimer);
                    spawnCoroutine = interactable.StartCoroutine(StartSpawnCountdown());
                }

                // loot being gone is a type of prerequisite for a lootable node
                // DISABLE MINIMAP ICON WHILE ITEM IS NOT SPAWNED

                HandleOptionStateChange();
            }
        }

        private void Despawn() {
            //Debug.Log($"{interactable.gameObject.name}.LootableNode.Despawn()");

            if (Props.SpawnObject != null && Props.SpawnObject.activeSelf == true) {
                Props.SpawnObject.SetActive(false);
            }
        }


        private void InitializeLootTableStates() {
            //Debug.Log($"{interactable.gameObject.name}.LootableNodeComponent.InitializeLootTableStates()");

            lootHolder.InitializeLootTableStates();
            foreach (LootTable lootTable in Props.LootTables) {
                lootHolder.AddLootTableState(lootTable);
            }
        }

        public override void StopInteract() {
            base.StopInteract();

            uIManager.lootWindow.CloseWindow();
        }

        public override bool CanInteract(UnitController sourceUnitController, bool processRangeCheck, bool passedRangeCheck, bool processNonCombatCheck, bool viaSwitch = false) {
            //Debug.Log(interactable.gameObject.name + ".LootableNode.CanInteract()");
            bool returnValue = base.CanInteract(sourceUnitController, processRangeCheck, passedRangeCheck, processNonCombatCheck);
            if (returnValue == false) {
                return false;
            }
            if (spawnCoroutine != null) {
                return false;
            }
            return (GetCurrentOptionCount(sourceUnitController) == 0 ? false : true);
        }
    }

}