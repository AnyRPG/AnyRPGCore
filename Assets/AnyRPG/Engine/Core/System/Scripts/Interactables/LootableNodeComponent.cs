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

        public LootHolder LootHolder { get => lootHolder; set => lootHolder = value; }

        public LootableNodeComponent(Interactable interactable, LootableNodeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            // initialize loot tables and states
            foreach (LootTable lootTable in Props.LootTables) {
                lootHolder.LootTableStates.Add(lootTable, new LootTableState());
            }
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
            SystemGameManager.Instance.UIManager.interactionWindow.CloseWindow();
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
                lootDrops.AddRange(lootTable.GetLoot(lootHolder.LootTableStates[lootTable]));
            }
            SystemGameManager.Instance.LootManager.CreatePages(lootDrops);
            lootDropped = true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>
        public void PickUp() {
            //Debug.Log(gameObject.name + ".LootableNode.Pickup()");
            //LootUI.Instance.CreatePages(lootTable.GetLoot());
            CreateWindowEventSubscriptions();
            SystemGameManager.Instance.UIManager.lootWindow.CloseableWindowContents.OnCloseWindow += ClearTakeLootHandler;
            SystemGameManager.Instance.UIManager.lootWindow.OpenWindow();
        }

        public void ClearTakeLootHandler(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".LootableNode.ClearTakeLootHandler()");
            CleanupWindowEventSubscriptions();
        }

        public void CreateWindowEventSubscriptions() {
            //Debug.Log(gameObject.name + ".LootableNode.CreateWindowEventSubscriptions()");
            SystemEventManager.StartListening("OnTakeLoot", HandleTakeLoot);
        }

        public void CleanupWindowEventSubscriptions() {
            //Debug.Log(gameObject.name + ".LootableNode.CleanupWindowEventSubscriptions()");
            SystemEventManager.StopListening("OnTakeLoot", HandleTakeLoot);
            if (SystemGameManager.Instance.UIManager?.lootWindow?.CloseableWindowContents != null) {
                SystemGameManager.Instance.UIManager.lootWindow.CloseableWindowContents.OnCloseWindow -= ClearTakeLootHandler;
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

        public void HandleTakeLoot(string eventName, EventParamProperties eventParamProperties) {
            CheckDropListSize();
        }

        public void ClearLootTables() {
            Props.LootTables.Clear();
        }

        public void CheckDropListSize() {
            //Debug.Log(gameObject.name + ".LootableNode.CheckDropListSize()");
            int lootCount = 0;
            foreach (LootTable lootTable in Props.LootTables) {
                lootCount += lootHolder.LootTableStates[lootTable].DroppedItems.Count;
            }
            if (lootCount == 0) {
                // since this method is only called on take loot, we can consider everything picked up if there is no loot left
                pickupCount++;

                lootDropped = false;
                //if (lootTable.MyDroppedItems.Count == 0) {
                SystemGameManager.Instance.PlayerManager.PlayerController.RemoveInteractable(interactable);
                interactable.DestroySpawn();
                foreach (LootTable lootTable in Props.LootTables) {
                    lootTable.Reset(lootHolder.LootTableStates[lootTable]);
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

            SystemGameManager.Instance.UIManager.lootWindow.CloseWindow();
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
            return (SystemGameManager.Instance.PlayerManager.MyCharacter.MyCharacterAbilityManager.HasAbility(MyAbility.MyName) == true && interactable.MySpawnReference != null ? 1 : 0);
        }
        */

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