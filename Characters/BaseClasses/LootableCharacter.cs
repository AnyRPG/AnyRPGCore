using AnyRPG;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    /// <summary>
    /// Currently enemy is a subclass of interactable to inherit right click and move toward functionality.
    /// This may be better implemented as an interface in the future.
    /// </summary>
    public class LootableCharacter : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyLootableCharacterInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyLootableCharacterInteractionPanelImage : base.MyIcon); }
        public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyLootableCharacterNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyLootableCharacterNamePlateImage : base.MyNamePlateImage); }

        /*
        [SerializeField]
        private LootTable lootTable;
        */

        [SerializeField]
        private List<string> lootTableNames = new List<string>();

        private List<LootTable> lootTables = new List<LootTable>();

        private CharacterUnit characterUnit;

        public CharacterUnit MyCharacterUnit { get => characterUnit; set => characterUnit = value; }
        public List<LootTable> MyLootTables { get => lootTables; set => lootTables = value; }

        protected override void Awake() {
            base.Awake();
        }

        protected override void Start() {
            base.Start();
            CreateEventSubscriptions();
        }

        public override void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".LootableCharacter.GetComponentReferences()");
            base.GetComponentReferences();
            characterUnit = GetComponent<CharacterUnit>();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            characterUnit.MyCharacter.MyCharacterStats.BeforeDie += HandleDeath;
            characterUnit.MyCharacter.MyCharacterStats.OnReviveComplete += HandleRevive;
            eventSubscriptionsInitialized = true;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".LootableCharacter.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterStats != null) {
                characterUnit.MyCharacter.MyCharacterStats.BeforeDie -= HandleDeath;
                characterUnit.MyCharacter.MyCharacterStats.OnReviveComplete -= HandleRevive;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnTakeLoot -= TryToDespawn;
            }
        }

        public override void OnDisable() {
            //Debug.Log(gameObject.name + ".LootableCharacter.OnDisable()");
            base.OnDisable();
            CleanupEventSubscriptions();
            ClearLootTables();
        }

        public void CreateLootTables() {
            //Debug.Log(gameObject.name + ".LootableCharacter.CreateLootTables()");
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

        public void HandleDeath(CharacterStats characterStats) {
            //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath()");
            if (PlayerManager.MyInstance == null) {
                // game is exiting
                return;
            }
            if (lootTableNames != null && characterStats.MyBaseCharacter.MyCharacterCombat.MyAggroTable.AggroTableContains(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit)) {
                //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath(): MyLootTable != null.  Getting loot");
                int lootCount = 0;
                foreach (LootTable lootTable in lootTables) {
                    if (lootTable != null) {
                        lootTable.GetLoot();
                        lootCount += lootTable.MyDroppedItems.Count;
                    }
                }
                if (lootCount > 0) {
                    //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath(): Loot count: " + MyLootTable.MyDroppedItems.Count + "; performing loot sparkle");

                    characterUnit.MyCharacter.MyCharacterAbilityManager.BeginAbility(SystemConfigurationManager.MyInstance.MyLootSparkleAbility as IAbility, gameObject);
                }
            } else {
                if (!characterStats.MyBaseCharacter.MyCharacterCombat.MyAggroTable.AggroTableContains(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit)) {
                    //Debug.Log(gameObject.name + ".LootableCharacter.HandleDeath(): Player not in agro table, no reason to drop loot.");
                }
                //Debug.Log(gameObject.name + ".LootableCharacter.HandleDeath(): MyLootTable == null. can't drop loot");
            }
            TryToDespawn();
        }

        public void TryToDespawn() {

            //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn()");
            if (MyCharacterUnit.MyCharacter.MyCharacterStats.IsAlive == true) {
                //Debug.Log("LootableCharacter.TryToDespawn(): Character is alive.  Returning and doing nothing.");
                return;
            }
            if (lootTables == null || lootTables.Count == 0) {
                //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): loot table was null, despawning");
                Despawn();
                return;
            }
            int lootCount = 0;
            foreach (LootTable lootTable in lootTables) {
                lootCount += lootTable.MyDroppedItems.Count;
            }
            if (lootCount == 0) {
                //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): loot table had no dropped items, despawning");
                SystemEventManager.MyInstance.OnTakeLoot -= TryToDespawn;

                // cancel loot sparkle here because despawn takes a while
                List<AbilityEffect> sparkleEffects = SystemConfigurationManager.MyInstance.MyLootSparkleAbility.MyAbilityEffects;
                foreach (AbilityEffect abilityEffect in sparkleEffects) {
                    //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): found a sparkle effect: " + SystemResourceManager.prepareStringForMatch(abilityEffect.MyName) + "; character effects: ");
                    if (characterUnit.MyBaseCharacter.MyCharacterStats.MyStatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(abilityEffect.MyName))) {
                        //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): found a sparkle effect: " + SystemResourceManager.prepareStringForMatch(abilityEffect.MyName) + " and now cancelling it");
                        characterUnit.MyBaseCharacter.MyCharacterStats.MyStatusEffects[SystemResourceManager.prepareStringForMatch(abilityEffect.MyName)].CancelStatusEffect();
                    }
                }

                Despawn();
            }

            // this is going here because if we didn't successfully despawn, we should check for loot and display minimap icon
            HandlePrerequisiteUpdates();

        }

        public override bool CanInteract() {
            //Debug.Log(gameObject.name + ".LootableCharacter.canInteract(" + (source == null ? "null" : source.MyName) + ")");
            if (base.CanInteract() == false) {
                //Debug.Log(gameObject.name + ".LootableCharacter.canInteract(): base.caninteract failed");
                return false;
            }

            int lootCount = GetLootCount();
            // changed this next line to getcurrentoptioncount to cover the size of the loot table and aliveness checks.  This should prevent an empty window from popping up after the character is looted
            if (lootTables != null && lootCount > 0 && GetCurrentOptionCount() > 0) {
                //Debug.Log(gameObject.name + ".LootableCharacter.canInteract(): isalive: false lootTable: " + lootTable.MyDroppedItems.Count);
                return true;
            }
            return false;
        }

        public int GetLootCount() {
            int lootCount = 0;
            foreach (LootTable lootTable in lootTables) {
                lootCount += lootTable.MyLoot.Count;
            }
            return lootCount;
        }

        public List<GameObject> GetLootableTargets() {
            Vector3 aoeSpawnCenter = MyCharacterUnit.transform.position;
            Collider[] colliders = new Collider[0];
            int validMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            colliders = Physics.OverlapSphere(aoeSpawnCenter, 15f, validMask);
            //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
            List<GameObject> validTargets = new List<GameObject>();
            foreach (Collider collider in colliders) {
                if (collider.gameObject.GetComponent<LootableCharacter>() != null) {
                    validTargets.Add(collider.gameObject);
                }
            }
            return validTargets;
        }

        public override bool Interact(CharacterUnit source) {
            Debug.Log(gameObject.name + ".LootableCharacter.Interact()");
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            if (!characterUnit.MyCharacter.MyCharacterStats.IsAlive) {
                Debug.Log(gameObject.name + ".LootableCharacter.Interact(): Character is dead.  Showing Loot Window on interaction");
                base.Interact(source);

                List<LootDrop> drops = new List<LootDrop>();
                foreach (GameObject interactable in GetLootableTargets()) {
                    LootableCharacter lootableCharacter = interactable.GetComponent<LootableCharacter>();
                    if (lootableCharacter != null) {
                        CharacterStats characterStats = interactable.GetComponent<CharacterUnit>().MyCharacter.MyCharacterStats as CharacterStats;
                        if (characterStats != null && characterStats.IsAlive == false && lootableCharacter.lootTables != null) {
                            //Debug.Log("Adding drops to loot table from: " + lootableCharacter.gameObject.name);
                            foreach (LootTable lootTable in lootableCharacter.MyLootTables) {
                                drops.AddRange(lootTable.GetLoot());
                                lootableCharacter.MonitorLootTable();
                            }
                        }
                    }
                }
                // that will ignore the current character because he will have been removed from the interactables list by death
                // this should take care of that situation
                //drops.AddRange(MyLootTable.GetLoot());
                // don't need anymore because of spherecast, not interactables

                if (drops.Count > 0) {
                    Debug.Log(gameObject.name + ".LootableCharacter.drops.Count: " + drops.Count);
                    LootUI.MyInstance.CreatePages(drops);
                    //Debug.Log(gameObject.name + ".LootableCharacter.Interact(): about to open window");
                    PopupWindowManager.MyInstance.lootWindow.OpenWindow();
                    return true;
                } else {
                    Debug.Log(gameObject.name + ".LootableCharacter.drops.Count: " + drops.Count);
                    return false;
                }
            }
            return false;
        }

        public void MonitorLootTable() {
            SystemEventManager.MyInstance.OnTakeLoot += TryToDespawn;
        }

        public void ClearTakeLootHandler(ICloseableWindowContents windowContents) {
            ClearTakeLootHandler();
        }

        public void ClearTakeLootHandler() {
            //Debug.Log(gameObject.name + ".LootableCharacter.ClearTakeLoothandler(): MyLootTable.MyDroppedItems.Count" + MyLootTable.MyDroppedItems.Count);
            TryToDespawn();
        }


        public override void StopInteract() {
            //Debug.Log(gameObject.name + ".LootableCharacter.StopInteract()");
            base.StopInteract();
            PopupWindowManager.MyInstance.lootWindow.CloseWindow();
            TryToDespawn();
        }

        public override bool HasMiniMapText() {
            return false;
        }

        public override bool SetMiniMapText(Text text) {
            if (!base.SetMiniMapText(text)) {
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            text.text = "o";
            text.color = Color.gray;
            text.fontSize = 50;
            return true;
        }

        public void Despawn() {
            //Debug.Log(gameObject.name + ".LootableCharacter.Despawn()");
            //gameObject.SetActive(false);
            if (MyCharacterUnit != null) {
                MyCharacterUnit.Despawn();
            }
        }

        public override int GetValidOptionCount() {
            // this was commented out.  putting it back in because bunnies are 
            //if (MyCharacterUnit != null && MyCharacterUnit.MyCharacter != null && MyCharacterUnit.MyCharacter.MyCharacterStats != null) {
                return (MyCharacterUnit.MyCharacter.MyCharacterStats.IsAlive == false ? 1 : 0);
            //}
            //return 0;
        }

        public override int GetCurrentOptionCount() {
            int lootCount = GetLootCount();
            return ((GetValidOptionCount() == 1 && lootCount > 0) ? 1 : 0);
        }

        public void HandleRevive() {
            ClearLootTable();
        }

        public void ClearLootTable() {
            foreach (LootTable lootTable in lootTables) {
                if (lootTable != null) {
                    lootTable.HandleRevive();
                }
            }
        }

        public override void HandlePrerequisiteUpdates() {
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override string GetSummary() {
            return "Lootable";
        }
    }

}