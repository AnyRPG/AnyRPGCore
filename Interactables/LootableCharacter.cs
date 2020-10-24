using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    /// <summary>
    /// Currently enemy is a subclass of interactable to inherit right click and move toward functionality.
    /// This may be better implemented as an interface in the future.
    /// </summary>
    public class LootableCharacter : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };
        public event System.Action<GameObject> OnLootComplete = delegate { };

        private LootableCharacterConfig lootableCharacterConfig;

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyLootableCharacterInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyLootableCharacterInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyLootableCharacterNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyLootableCharacterNamePlateImage : base.NamePlateImage); }

        
        [Header("Loot")]

        [Tooltip("If true, when killed, this unit will drop the system defined currency amount for its level and toughness")]
        [SerializeField]
        private bool automaticCurrency = false;

        [Tooltip("Define items that can drop in this list")]
        [SerializeField]
        private List<string> lootTableNames = new List<string>();

        private List<LootTable> lootTables = new List<LootTable>();

        private CharacterUnit characterUnit;

        // keep track of whether or not currency for this character has been rolled.
        private bool currencyRolled = false;
        private bool currencyCollected = false;

        private bool lootCalculated = false;

        // hold the rolled currency amount
        private CurrencyNode currencyNode;

        public CharacterUnit MyCharacterUnit { get => characterUnit; set => characterUnit = value; }
        public List<LootTable> MyLootTables { get => lootTables; set => lootTables = value; }
        public bool AutomaticCurrency { get => automaticCurrency; set => automaticCurrency = value; }
        public bool CurrencyRolled { get => currencyRolled; }
        public CurrencyNode CurrencyNode { get => currencyNode; set => currencyNode = value; }
        public bool CurrencyCollected { get => currencyCollected; set => currencyCollected = value; }

        public LootableCharacter(Interactable interactable, LootableCharacterConfig interactableConfig) : base(interactable) {
            this.lootableCharacterConfig = interactableConfig;
        }

        protected override void Start() {
            base.Start();
            AddUnitProfileSettings();
        }

        public void AddUnitProfileSettings() {
            if (characterUnit != null && characterUnit.BaseCharacter != null && characterUnit.BaseCharacter.UnitProfile != null) {
                if (characterUnit.BaseCharacter.UnitProfile.LootTableNames != null) {
                    foreach (string lootTableName in characterUnit.BaseCharacter.UnitProfile.LootTableNames) {
                        LootTable lootTable = SystemLootTableManager.MyInstance.GetNewResource(lootTableName);
                        if (lootTable != null) {
                            lootTables.Add(lootTable);
                        }
                    }
                }
                automaticCurrency = characterUnit.BaseCharacter.UnitProfile.AutomaticCurrency;
            }
            HandlePrerequisiteUpdates();

        }


        public override void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".LootableCharacter.GetComponentReferences()");
            base.GetComponentReferences();
            characterUnit = GetComponent<CharacterUnit>();
        }

        public override void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".LootableCharacter.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            //Debug.Log(gameObject.name + ".LootableCharacter.CreateEventSubscriptions(): subscribing to handledeath");
            characterUnit.BaseCharacter.CharacterStats.BeforeDie += HandleDeath;
            characterUnit.BaseCharacter.CharacterStats.OnReviveComplete += HandleRevive;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".LootableCharacter.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            if (characterUnit != null && characterUnit.BaseCharacter != null && characterUnit.BaseCharacter.CharacterStats != null) {
                characterUnit.BaseCharacter.CharacterStats.BeforeDie -= HandleDeath;
                characterUnit.BaseCharacter.CharacterStats.OnReviveComplete -= HandleRevive;
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
            int lootCount = 0;
            //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath(): MyLootTable != null.  Getting loot");
            //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath(): characterinAgrotable: " + characterUnit.BaseCharacter.CharacterCombat.MyAggroTable.AggroTableContains(PlayerManager.MyInstance.MyCharacter.CharacterUnit));
            if (lootTables != null && characterUnit.BaseCharacter.CharacterCombat.MyAggroTable.AggroTableContains(PlayerManager.MyInstance.MyCharacter.CharacterUnit)) {
                //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath(): characterinAgrotable: " + characterUnit.BaseCharacter.CharacterCombat.MyAggroTable.AggroTableContains(PlayerManager.MyInstance.MyCharacter.CharacterUnit));
                lootCount = GetLootCount();
            }
            lootCalculated = true;
            if (lootCount > 0) {
                //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath(): Loot count: " + MyLootTable.MyDroppedItems.Count + "; performing loot sparkle");

                //SystemAbilityController.MyInstance.BeginAbility(SystemConfigurationManager.MyInstance.MyLootSparkleAbility as IAbility, gameObject);
                AbilityEffectContext abilityEffectContext = new AbilityEffectContext();
                abilityEffectContext.baseAbility = SystemConfigurationManager.MyInstance.LootSparkleAbility;
                SystemConfigurationManager.MyInstance.LootSparkleAbility.Cast(SystemAbilityController.MyInstance, gameObject, new AbilityEffectContext());
            }
            TryToDespawn();
        }

        public void TryToDespawn() {

            //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn()");
            if (MyCharacterUnit.BaseCharacter.CharacterStats.IsAlive == true) {
                //Debug.Log("LootableCharacter.TryToDespawn(): Character is alive.  Returning and doing nothing.");
                return;
            }
            if (lootTables == null || lootTables.Count == 0) {
                //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): loot table was null, despawning");
                if (gameObject != null) {
                    OnLootComplete(gameObject);
                }
                Despawn();
                return;
            }
            int lootCount = GetLootCount();
            if (lootCount == 0) {
                //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): loot table had no dropped items, despawning");
                SystemEventManager.MyInstance.OnTakeLoot -= TryToDespawn;

                // cancel loot sparkle here because despawn takes a while
                List<AbilityEffect> sparkleEffects = SystemConfigurationManager.MyInstance.LootSparkleAbility.AbilityEffects;
                foreach (AbilityEffect abilityEffect in sparkleEffects) {
                    //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): found a sparkle effect: " + SystemResourceManager.prepareStringForMatch(abilityEffect.MyName) + "; character effects: ");
                    if (characterUnit.BaseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(abilityEffect.DisplayName))) {
                        //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): found a sparkle effect: " + SystemResourceManager.prepareStringForMatch(abilityEffect.MyName) + " and now cancelling it");
                        characterUnit.BaseCharacter.CharacterStats.StatusEffects[SystemResourceManager.prepareStringForMatch(abilityEffect.DisplayName)].CancelStatusEffect();
                    }
                }
                OnLootComplete(gameObject);
                Despawn();
            }/* else {
                // 2. moved here from below to prevent dead units that have been looted from re-displaying their health bar
                HandlePrerequisiteUpdates();

            }*/

            // 1. this is going here because if we didn't successfully despawn, we should check for loot and display minimap icon
            //HandlePrerequisiteUpdates();

            // 3. re-enabled because this is going here because if we didn't successfully despawn, we should check for loot and display minimap icon or hide it (if no loot left)
            // also, there is now code in the healthbar update that detects if the unit spawns dead, and if not, will not re-enable the healthbar
            // having it in the else block caused units that had been looted and have no loot left, and are now on despawn countdown to still display the minimap icon
            HandlePrerequisiteUpdates();


        }

        public override bool CanInteract() {
            //Debug.Log(gameObject.name + ".LootableCharacter.canInteract(" + (source == null ? "null" : source.MyName) + ")");
            if (base.CanInteract() == false || GetCurrentOptionCount() == 0) {
                //Debug.Log(gameObject.name + ".LootableCharacter.canInteract(): base.caninteract failed");
                return false;
            }

            int lootCount = GetLootCount();
            // changed this next line to getcurrentoptioncount to cover the size of the loot table and aliveness checks.  This should prevent an empty window from popping up after the character is looted
            if (lootTables != null && lootCount > 0) {
                //Debug.Log(gameObject.name + ".LootableCharacter.canInteract(): isalive: false lootTable: " + lootTable.MyDroppedItems.Count);
                return true;
            }
            return false;
        }

        public int GetLootCount() {
            //Debug.Log(gameObject.name + ".LootableCharacter.GetLootCount()");
            int lootCount = 0;
            
            foreach (LootTable lootTable in lootTables) {
                if (lootTable != null) {
                    lootTable.GetLoot(!lootCalculated);
                    lootCount += lootTable.MyDroppedItems.Count;
                    //Debug.Log(gameObject.name + ".LootableCharacter.GetLootCount(): after loot table count: " + lootCount);
                }
            }
            if (AutomaticCurrency == true) {
                //Debug.Log(gameObject.name + ".LootableCharacter.Interact(): automatic currency : true");
                CurrencyNode tmpNode = GetCurrencyLoot();
                if (tmpNode.currency != null) {
                    lootCount += 1;
                    //Debug.Log(gameObject.name + ".LootableCharacter.GetLootCount(): after currency count: " + lootCount);
                }
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

        public void TakeCurrencyLoot() {
            currencyCollected = true;
            currencyNode = new CurrencyNode();
        }

        public CurrencyNode GetCurrencyLoot() {
            //Debug.Log(gameObject.name + ".LootableCharacter.GetCurrencyLoot()");
            if (currencyRolled == true || lootCalculated == true) {
                //Debug.Log(gameObject.name + ".LootableCharacter.GetCurrencyLoot(): currencyRolled: " + currencyRolled + "; lootCalculated: " + lootCalculated);
                return currencyNode;
            }
            if (automaticCurrency == true) {
                //Debug.Log(gameObject.name + ".LootableCharacter.GetCurrencyLoot(): automatic is true");
                currencyNode.currency = SystemConfigurationManager.MyInstance.KillCurrency;
                if ((namePlateUnit as CharacterUnit) is CharacterUnit) {
                    currencyNode.MyAmount = SystemConfigurationManager.MyInstance.KillCurrencyAmountPerLevel * (namePlateUnit as CharacterUnit).BaseCharacter.CharacterStats.Level;
                    if ((namePlateUnit as CharacterUnit).BaseCharacter.CharacterStats.Toughness != null) {
                        currencyNode.MyAmount *= (int)(namePlateUnit as CharacterUnit).BaseCharacter.CharacterStats.Toughness.CurrencyMultiplier;
                    }
                }
            }
            //Debug.Log(gameObject.name + ".LootableCharacter.GetCurrencyLoot(): setting currency rolled to true");
            currencyRolled = true;
            //Debug.Log(gameObject.name + ".LootableCharacter.GetCurrencyLoot(): returning currency: " + currencyNode.currency.MyDisplayName + "; amount: " + currencyNode.MyAmount);
            return currencyNode;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".LootableCharacter.Interact()");
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            if (!characterUnit.BaseCharacter.CharacterStats.IsAlive) {
                //Debug.Log(gameObject.name + ".LootableCharacter.Interact(): Character is dead.  Showing Loot Window on interaction");
                base.Interact(source);
                // keep track of currency drops for combining after
                CurrencyLootDrop droppedCurrencies = new CurrencyLootDrop();

                List<LootDrop> drops = new List<LootDrop>();
                List<LootDrop> itemDrops = new List<LootDrop>();
                foreach (GameObject interactable in GetLootableTargets()) {
                    LootableCharacter lootableCharacter = interactable.GetComponent<LootableCharacter>();
                    if (lootableCharacter != null) {
                        CharacterStats characterStats = interactable.GetComponent<CharacterUnit>().BaseCharacter.CharacterStats as CharacterStats;
                        if (characterStats != null && characterStats.IsAlive == false && lootableCharacter.lootTables != null) {
                            //Debug.Log("Adding drops to loot table from: " + lootableCharacter.gameObject.name);

                            // get currency loot
                            if (lootableCharacter.AutomaticCurrency == true) {
                                //Debug.Log(gameObject.name + ".LootableCharacter.Interact(): automatic currency : true");
                                CurrencyNode tmpNode = lootableCharacter.GetCurrencyLoot();
                                if (tmpNode.currency != null) {
                                    droppedCurrencies.AddCurrencyNode(lootableCharacter, tmpNode);
                                }
                            }

                            // get item loot
                            foreach (LootTable lootTable in lootableCharacter.MyLootTables) {
                                itemDrops.AddRange(lootTable.GetLoot());
                                lootableCharacter.MonitorLootTable();
                            }
                        }
                    }
                }
                // that will ignore the current character because he will have been removed from the interactables list by death
                // this should take care of that situation
                //drops.AddRange(MyLootTable.GetLoot());
                // don't need anymore because of spherecast, not interactables

                // combine all currencies from all lootable targets in range into a single currency lootdrop, and add that as the first lootdrop
                // in the drops list so it shows up on the first page as the first item
                if (droppedCurrencies.CurrencyNodes.Count > 0) {
                    drops.Add(droppedCurrencies);
                }
                drops.AddRange(itemDrops);

                if (drops.Count > 0) {
                    //Debug.Log(gameObject.name + ".LootableCharacter.drops.Count: " + drops.Count);
                    LootUI.MyInstance.CreatePages(drops);
                    //Debug.Log(gameObject.name + ".LootableCharacter.Interact(): about to open window");
                    PopupWindowManager.MyInstance.lootWindow.OpenWindow();
                    return true;
                } else {
                    //Debug.Log(gameObject.name + ".LootableCharacter.drops.Count: " + drops.Count);
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

        public override bool SetMiniMapText(TextMeshProUGUI text) {
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
                return (MyCharacterUnit.BaseCharacter.CharacterStats.IsAlive == false ? 1 : 0);
            //}
            //return 0;
        }

        public override int GetCurrentOptionCount() {
            if (GetValidOptionCount() == 0) {
                return 0;
            }
            int lootCount = GetLootCount();
            return ((lootCount > 0) ? 1 : 0);
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

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


        public override string GetSummary() {
            return "Lootable";
        }
    }

}