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
    public class LootableCharacterComponent : InteractableOptionComponent {

        public event System.Action<UnitController> OnLootComplete = delegate { };

        public LootableCharacterProps Props { get => interactableOptionProps as LootableCharacterProps; }

        private CharacterUnit characterUnit;

        // keep track of whether or not currency for this character has been rolled.
        private bool currencyRolled = false;
        private bool currencyCollected = false;

        private bool lootCalculated = false;

        // hold the rolled currency amount
        private CurrencyNode currencyNode;

        private List<LootTable> lootTables = new List<LootTable>();

        public CharacterUnit MyCharacterUnit { get => characterUnit; set => characterUnit = value; }
        public bool CurrencyRolled { get => currencyRolled; }
        public CurrencyNode CurrencyNode { get => currencyNode; set => currencyNode = value; }
        public bool CurrencyCollected { get => currencyCollected; set => currencyCollected = value; }
        public List<LootTable> LootTables { get => lootTables; }

        public LootableCharacterComponent(Interactable interactable, LootableCharacterProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            CreateLootTables();
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = "Loot";
            }

        }

        public static LootableCharacterComponent GetLootableCharacterComponent(Interactable searchInteractable) {
            return searchInteractable.GetFirstInteractableOption(typeof(LootableCharacterComponent)) as LootableCharacterComponent;
        }

        public override void Cleanup() {
            base.Cleanup();
            ClearLootTables();
        }

        /*
        protected override void AddUnitProfileSettings() {
            base.AddUnitProfileSettings();
            if (unitProfile != null) {
                if (unitProfile.LootableCharacterProps != null) {
                    interactableOptionProps = unitProfile.LootableCharacterProps;
                }
            }
            HandlePrerequisiteUpdates();
        }
        */


        public override void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".LootableCharacter.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();

            // have to set this here instead of constructor because the base constructor will call this before the local constructor runs
            characterUnit = CharacterUnit.GetCharacterUnit(interactable);

            (characterUnit.Interactable as UnitController).OnBeforeDie += HandleDeath;
            (characterUnit.Interactable as UnitController).OnReviveComplete += HandleRevive;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".LootableCharacter.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            if (characterUnit != null && characterUnit.BaseCharacter != null && characterUnit.BaseCharacter.CharacterStats != null) {
                (characterUnit.Interactable as UnitController).OnBeforeDie -= HandleDeath;
                (characterUnit.Interactable as UnitController).OnReviveComplete -= HandleRevive;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnTakeLoot -= TryToDespawn;
            }
        }

        public void CreateLootTables() {
            //Debug.Log(gameObject.name + ".LootableCharacter.CreateLootTables()");
            foreach (string lootTableName in Props.LootTableNames) {
                LootTable lootTable = SystemLootTableManager.MyInstance.GetNewResource(lootTableName);
                if (lootTable != null) {
                    lootTables.Add(lootTable);
                }
            }
        }

        public void ClearLootTables() {
            lootTables.Clear();
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
            if (lootTables != null && characterUnit.BaseCharacter.CharacterCombat.AggroTable.AggroTableContains(PlayerManager.MyInstance.UnitController.CharacterUnit)) {
                //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath(): characterinAgrotable: " + characterUnit.BaseCharacter.CharacterCombat.MyAggroTable.AggroTableContains(PlayerManager.MyInstance.MyCharacter.CharacterUnit));
                lootCount = GetLootCount();
            }
            lootCalculated = true;
            if (lootCount > 0 && SystemConfigurationManager.Instance?.LootSparkleEffect != null) {
                //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath(): Loot count: " + MyLootTable.MyDroppedItems.Count + "; performing loot sparkle");

                //SystemAbilityController.MyInstance.BeginAbility(SystemConfigurationManager.MyInstance.MyLootSparkleAbility as IAbility, gameObject);
                SystemConfigurationManager.Instance.LootSparkleEffect.Cast(SystemAbilityController.MyInstance, interactable, interactable, new AbilityEffectContext());
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
                if (interactable != null && interactable.gameObject != null) {
                    AdvertiseLootComplete();
                }
                Despawn();
                return;
            }
            int lootCount = GetLootCount();
            if (lootCount == 0) {
                //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): loot table had no dropped items, despawning");
                SystemEventManager.MyInstance.OnTakeLoot -= TryToDespawn;

                // cancel loot sparkle here because despawn takes a while
                if (characterUnit.BaseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(SystemConfigurationManager.Instance.LootSparkleEffect.DisplayName))) {
                    //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): found a sparkle effect: " + SystemResourceManager.prepareStringForMatch(abilityEffect.MyName) + " and now cancelling it");
                    characterUnit.BaseCharacter.CharacterStats.StatusEffects[SystemResourceManager.prepareStringForMatch(SystemConfigurationManager.Instance.LootSparkleEffect.DisplayName)].CancelStatusEffect();
                }
                AdvertiseLootComplete();
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

        public void AdvertiseLootComplete() {
            if (interactable != null) {
                UnitController unitController = interactable.gameObject.GetComponent<UnitController>();
                if (unitController != null) {
                    OnLootComplete(unitController);
                }
            }
        }

        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false, float factionValue = 0f, bool processNonCombatCheck = true) {
            //Debug.Log(gameObject.name + ".LootableCharacter.canInteract(" + (source == null ? "null" : source.MyName) + ")");

            // you can't loot friendly characters
            if (factionValue > -1f ) {
                return false;
            }
            // testing, this could include a a range check, and we want that to be processed, so send in a fake faction value
            //if (base.CanInteract(processRangeCheck, passedRangeCheck, factionValue) == false || GetCurrentOptionCount() == 0) {
            if (base.CanInteract(processRangeCheck, passedRangeCheck, 0f, false) == false || GetCurrentOptionCount() == 0) {
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
            if (Props.AutomaticCurrency == true) {
                //Debug.Log(gameObject.name + ".LootableCharacter.Interact(): automatic currency : true");
                CurrencyNode tmpNode = GetCurrencyLoot();
                if (tmpNode.currency != null) {
                    lootCount += 1;
                    //Debug.Log(gameObject.name + ".LootableCharacter.GetLootCount(): after currency count: " + lootCount);
                }
            }
            return lootCount;
        }

        public List<Interactable> GetLootableTargets() {
            Vector3 aoeSpawnCenter = MyCharacterUnit.Interactable.transform.position;
            Collider[] colliders = new Collider[0];
            int validMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            colliders = Physics.OverlapSphere(aoeSpawnCenter, 15f, validMask);
            //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
            List<Interactable> validTargets = new List<Interactable>();
            foreach (Collider collider in colliders) {
                Interactable _interactable = collider.gameObject.GetComponent<Interactable>();
                if (_interactable != null) {
                    validTargets.Add(_interactable);
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
            if (Props.AutomaticCurrency == true) {
                //Debug.Log(gameObject.name + ".LootableCharacter.GetCurrencyLoot(): automatic is true");
                currencyNode.currency = SystemConfigurationManager.Instance.KillCurrency;
                if (characterUnit != null) {
                    currencyNode.MyAmount = SystemConfigurationManager.Instance.KillCurrencyAmountPerLevel * characterUnit.BaseCharacter.CharacterStats.Level;
                    if (characterUnit.BaseCharacter.CharacterStats.Toughness != null) {
                        currencyNode.MyAmount *= (int)characterUnit.BaseCharacter.CharacterStats.Toughness.CurrencyMultiplier;
                    }
                }
            }
            //Debug.Log(gameObject.name + ".LootableCharacter.GetCurrencyLoot(): setting currency rolled to true");
            currencyRolled = true;
            //Debug.Log(gameObject.name + ".LootableCharacter.GetCurrencyLoot(): returning currency: " + currencyNode.currency.MyDisplayName + "; amount: " + currencyNode.MyAmount);
            return currencyNode;
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(interactable.gameObject.name + ".LootableCharacter.Interact()");
            PopupWindowManager.Instance.interactionWindow.CloseWindow();
            if (!characterUnit.BaseCharacter.CharacterStats.IsAlive) {
                //Debug.Log(gameObject.name + ".LootableCharacter.Interact(): Character is dead.  Showing Loot Window on interaction");
                base.Interact(source, optionIndex);
                // keep track of currency drops for combining after
                CurrencyLootDrop droppedCurrencies = new CurrencyLootDrop();

                List<LootDrop> drops = new List<LootDrop>();
                List<LootDrop> itemDrops = new List<LootDrop>();
                foreach (Interactable interactable in GetLootableTargets()) {
                    LootableCharacterComponent lootableCharacter = LootableCharacterComponent.GetLootableCharacterComponent(interactable);
                    if (lootableCharacter != null) {
                        CharacterStats characterStats = CharacterUnit.GetCharacterUnit(interactable).BaseCharacter.CharacterStats as CharacterStats;
                        if (characterStats != null && characterStats.IsAlive == false && lootableCharacter.LootTables != null) {
                            //Debug.Log("Adding drops to loot table from: " + lootableCharacter.gameObject.name);

                            // get currency loot
                            if (Props.AutomaticCurrency == true) {
                                //Debug.Log(gameObject.name + ".LootableCharacter.Interact(): automatic currency : true");
                                CurrencyNode tmpNode = lootableCharacter.GetCurrencyLoot();
                                if (tmpNode.currency != null) {
                                    droppedCurrencies.AddCurrencyNode(lootableCharacter, tmpNode);
                                }
                            }

                            // get item loot
                            foreach (LootTable lootTable in lootableCharacter.LootTables) {
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
                    //Debug.Log(interactable.gameObject.name + ".LootableCharacter.drops.Count: " + drops.Count);
                    LootManager.MyInstance.CreatePages(drops);
                    //Debug.Log(gameObject.name + ".LootableCharacter.Interact(): about to open window");
                    PopupWindowManager.Instance.lootWindow.OpenWindow();
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
            PopupWindowManager.Instance.lootWindow.CloseWindow();
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
            if (base.GetValidOptionCount() == 0) {
                return 0;
            }
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

        public override string GetSummary() {
            return "Lootable";
        }
        
    }

}