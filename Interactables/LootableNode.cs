using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootableNode : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        protected List<string> lootTableNames = new List<string>();

        protected List<LootTable> lootTables = new List<LootTable>();

        [SerializeField]
        protected float spawnTimer = 5f;

        protected float currentTimer = 0f;

        protected bool lootDropped = false;


        protected override void Awake() {
            //Debug.Log(gameObject.name + ".GatheringNode.Awake();");
            base.Awake();
        }

        public override bool Interact(CharacterUnit source) {
            if (lootTableNames == null) {
                //Debug.Log(gameObject.name + ".GatheringNode.Interact(" + source.name + "): lootTable was null!");
                return true;
            }
            base.Interact(source);

            DropLoot();
            PickUp();
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            return true;
        }

        protected IEnumerator StartSpawnCountdown() {
            //Debug.Log(gameObject.name + ".GatheringNode.StartSpawnCountdown()");
            // DISABLE MINIMAP ICON WHILE ITEM IS NOT SPAWNED
            HandlePrerequisiteUpdates();
            currentTimer = spawnTimer;
            while (currentTimer > 0) {
                //Debug.Log("Spawn Timer: " + currentTimer);
                yield return new WaitForSeconds(1);
                currentTimer -= 1;
            }
            interactable.Spawn();
            // ENABLE MINIMAP ICON AFTER SPAWN
            HandlePrerequisiteUpdates();
        }


        public virtual void DropLoot() {
            List<LootDrop> lootDrops = new List<LootDrop>();
            foreach (LootTable lootTable in lootTables) {
                lootDrops.AddRange(lootTable.GetLoot());
            }
            LootUI.MyInstance.CreatePages(lootDrops);
            lootDropped = true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>
        public void PickUp() {
            //Debug.Log("GatheringNode.Pickup()");
            //LootUI.MyInstance.CreatePages(lootTable.GetLoot());
            CreateEventSubscriptions();
            LootUI.MyInstance.OnCloseWindow += ClearTakeLootHandler;
            PopupWindowManager.MyInstance.lootWindow.OpenWindow();
        }

        public void ClearTakeLootHandler(ICloseableWindowContents windowContents) {
            CleanupWindowEventSubscriptions();
        }

        public override void CreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            SystemEventManager.MyInstance.OnTakeLoot += CheckDropListSize;
            //LootUI.MyInstance.OnCloseWindow += ClearTakeLootHandler;
            eventSubscriptionsInitialized = true;
        }

        public void CleanupWindowEventSubscriptions() {
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

        public void CreateLootTables() {
            foreach (string lootTableName in lootTableNames) {
                LootTable lootTable = SystemLootTableManager.MyInstance.GetNewResource(lootTableName);
                if (lootTable != null) {
                    lootTables.Add(lootTable);
                }
            }
        }

        public void ClearLootTables() {
            lootTables.Clear();
        }

        public void OnEnable() {
            CreateLootTables();
        }

        public override void OnDisable() {
            base.OnDisable();
            CleanupEventSubscriptions();
            StopAllCoroutines();
            ClearLootTables();
        }

        public void CheckDropListSize() {
            //Debug.Log("GatheringNode.CheckDropListSize()");
            int lootCount = 0;
            foreach (LootTable lootTable in lootTables) {
                lootCount += lootTable.MyDroppedItems.Count;
            }
            if (lootCount == 0) {
                lootDropped = false;
                //if (lootTable.MyDroppedItems.Count == 0) {
                (PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).RemoveInteractable(gameObject.GetComponent<Interactable>());
                interactable.DestroySpawn();
                foreach (LootTable lootTable in lootTables) {
                    lootTable.Reset();
                }
                StartCoroutine(StartSpawnCountdown());
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

        /*
        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".GatheringNode.GetCurrentOptionCount()");
            return (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(MyAbility.MyName) == true && interactable.MySpawnReference != null ? 1 : 0);
        }
        */

        public override void HandlePrerequisiteUpdates() {
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


        public override bool CanInteract() {
            bool returnValue = base.CanInteract();
            if (returnValue == false) {
                return false;
            }
            return (GetCurrentOptionCount() == 0 ? false : true);
        }
    }

}