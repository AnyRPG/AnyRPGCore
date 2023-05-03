using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootableCharacterComponent : InteractableOptionComponent, ILootHolder {

        public event System.Action<UnitController> OnLootComplete = delegate { };

        public LootableCharacterProps Props { get => interactableOptionProps as LootableCharacterProps; }

        private CharacterUnit characterUnit;

        // keep track of whether or not currency for this character has been rolled.
        private bool currencyRolled = false;
        private bool currencyCollected = false;

        private bool lootCalculated = false;

        private bool monitoringTakeLoot = false;

        // hold the rolled currency amount
        private CurrencyNode currencyNode;

        private LootHolder lootHolder = new LootHolder();

        // game manager references
        private SystemAbilityController systemAbilityController = null;
        private LootManager lootManager = null;

        public CharacterUnit CharacterUnit { get => characterUnit; set => characterUnit = value; }
        public bool CurrencyRolled { get => currencyRolled; }
        public CurrencyNode CurrencyNode { get => currencyNode; set => currencyNode = value; }
        public bool CurrencyCollected { get => currencyCollected; set => currencyCollected = value; }
        public LootHolder LootHolder { get => lootHolder; set => lootHolder = value; }

        public LootableCharacterComponent(Interactable interactable, LootableCharacterProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            CreateLootTables();
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = "Loot";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemAbilityController = systemGameManager.SystemAbilityController;
            lootManager = systemGameManager.LootManager;
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


        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.LootableCharacter.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            // have to set this here instead of constructor because the base constructor will call this before the local constructor runs
            characterUnit = CharacterUnit.GetCharacterUnit(interactable);

            (characterUnit.Interactable as UnitController).UnitEventController.OnBeforeDie += HandleDeath;
            //(characterUnit.Interactable as UnitController).UnitEventController.OnReviveComplete += HandleRevive;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.LootableCharacter.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            if (characterUnit != null && characterUnit.BaseCharacter != null && characterUnit.BaseCharacter.CharacterStats != null) {
                (characterUnit.Interactable as UnitController).UnitEventController.OnBeforeDie -= HandleDeath;
                //(characterUnit.Interactable as UnitController).UnitEventController.OnReviveComplete -= HandleRevive;
            }
            if (monitoringTakeLoot) {
                SystemEventManager.StopListening("OnTakeLoot", HandleTakeLoot);
                monitoringTakeLoot = false;
            }
        }

        public void HandleTakeLoot(string eventName, EventParamProperties eventParamProperties) {
            TryToDespawn();
        }

        public void CreateLootTables() {
            //Debug.Log($"{gameObject.name}.LootableCharacter.CreateLootTables()");
            foreach (string lootTableName in Props.LootTableNames) {
                LootTable lootTable = systemDataFactory.GetResource<LootTable>(lootTableName);
                if (lootTable != null) {
                    lootHolder.AddLootTableState(lootTable, new LootTableState(systemGameManager));
                }
            }
        }

        public void ClearLootTables() {
            //Debug.Log(interactable.gameObject.name + "LootableCharacterComponent.ClearLootTables()");
            ResetLootTableStates();
            lootHolder.ClearLootTableStates();
        }

        public void HandleDeath(CharacterStats characterStats) {
            //Debug.Log(interactable.gameObject.name + "LootableCharacter.HandleDeath()");
            if (playerManager == null) {
                // game is exiting
                return;
            }
            int lootCount = 0;
            //Debug.Log($"{gameObject.name}LootableCharacter.HandleDeath(): MyLootTable != null.  Getting loot");
            //Debug.Log($"{gameObject.name}LootableCharacter.HandleDeath(): characterinAgrotable: " + characterUnit.BaseCharacter.CharacterCombat.MyAggroTable.AggroTableContains(playerManager.MyCharacter.CharacterUnit));
            if (LootHolder.LootTableStates.Count > 0 && characterUnit.BaseCharacter.CharacterCombat.AggroTable.AggroTableContains(playerManager.UnitController.CharacterUnit)) {
                //Debug.Log($"{gameObject.name}LootableCharacter.HandleDeath(): characterinAgrotable: " + characterUnit.BaseCharacter.CharacterCombat.MyAggroTable.AggroTableContains(playerManager.MyCharacter.CharacterUnit));
                lootCount = GetLootCount();
            }
            lootCalculated = true;
            if (lootCount > 0 && systemConfigurationManager.LootSparkleEffect != null) {
                //Debug.Log($"{gameObject.name}LootableCharacter.HandleDeath(): Loot count: " + MyLootTable.MyDroppedItems.Count + "; performing loot sparkle");

                //systemAbilityController.BeginAbility(systemConfigurationManager.MyLootSparkleAbility as IAbility, gameObject);
                systemConfigurationManager.LootSparkleEffect.AbilityEffectProperties.Cast(systemAbilityController, interactable, interactable, new AbilityEffectContext());
            }
            TryToDespawn();
        }

        public void TryToDespawn() {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.TryToDespawn()");

            if (CharacterUnit.BaseCharacter.CharacterStats.IsAlive == true) {
                //Debug.Log("LootableCharacter.TryToDespawn(): Character is alive.  Returning and doing nothing.");
                return;
            }
            if (LootHolder.LootTableStates.Count == 0) {
                //Debug.Log($"{gameObject.name}.LootableCharacter.TryToDespawn(): loot table was null, despawning");
                if (interactable != null && interactable.gameObject != null) {
                    AdvertiseLootComplete();
                }
                Despawn();
                return;
            }
            int lootCount = GetLootCount();
            if (lootCount == 0) {
                //Debug.Log($"{gameObject.name}.LootableCharacter.TryToDespawn(): loot table had no dropped items, despawning");
                if (monitoringTakeLoot) {
                    SystemEventManager.StopListening("OnTakeLoot", HandleTakeLoot);
                    monitoringTakeLoot = false;
                }

                // cancel loot sparkle here because despawn takes a while
                if (characterUnit.BaseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemDataUtility.PrepareStringForMatch(systemConfigurationManager.LootSparkleEffect.ResourceName))) {
                    characterUnit.BaseCharacter.CharacterStats.StatusEffects[SystemDataUtility.PrepareStringForMatch(systemConfigurationManager.LootSparkleEffect.ResourceName)].CancelStatusEffect();
                }
                AdvertiseLootComplete();
                Despawn();
            }
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

            // you can't loot friendly characters
            if (factionValue > -1f ) {
                return false;
            }
            // testing, this could include a a range check, and we want that to be processed, so send in a fake faction value
            if (base.CanInteract(processRangeCheck, passedRangeCheck, 0f, false) == false || GetCurrentOptionCount() == 0) {
                return false;
            }

            int lootCount = GetLootCount();
            // changed this next line to getcurrentoptioncount to cover the size of the loot table and aliveness checks.  This should prevent an empty window from popping up after the character is looted
            if (lootHolder.LootTableStates.Count > 0 && lootCount > 0) {
                return true;
            }
            return false;
        }

        public int GetLootCount() {
            //Debug.Log(interactable.gameObject.name + ".LootableCharacter.GetLootCount()");
            int lootCount = 0;
            
            foreach (LootTable lootTable in lootHolder.LootTableStates.Keys) {
                if (lootTable != null) {
                    lootHolder.LootTableStates[lootTable].GetLoot(lootTable, !lootCalculated);
                    lootCount += lootHolder.LootTableStates[lootTable].DroppedItems.Count;
                    //Debug.Log($"{gameObject.name}.LootableCharacter.GetLootCount(): after loot table count: " + lootCount);
                }
            }
            if (Props.AutomaticCurrency == true) {
                //Debug.Log($"{gameObject.name}.LootableCharacter.Interact(): automatic currency : true");
                CurrencyNode tmpNode = GetCurrencyLoot();
                if (tmpNode.currency != null) {
                    lootCount += 1;
                    //Debug.Log($"{gameObject.name}.LootableCharacter.GetLootCount(): after currency count: " + lootCount);
                }
            }
            return lootCount;
        }

        public List<Interactable> GetLootableTargets() {
            Vector3 aoeSpawnCenter = CharacterUnit.Interactable.transform.position;
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
            //Debug.Log($"{gameObject.name}.LootableCharacter.GetCurrencyLoot()");
            if (currencyRolled == true || lootCalculated == true) {
                //Debug.Log($"{gameObject.name}.LootableCharacter.GetCurrencyLoot(): currencyRolled: " + currencyRolled + "; lootCalculated: " + lootCalculated);
                return currencyNode;
            }
            if (Props.AutomaticCurrency == true) {
                //Debug.Log($"{gameObject.name}.LootableCharacter.GetCurrencyLoot(): automatic is true");
                currencyNode.currency = systemConfigurationManager.KillCurrency;
                if (characterUnit != null) {
                    currencyNode.Amount = systemConfigurationManager.KillCurrencyAmountPerLevel * characterUnit.BaseCharacter.CharacterStats.Level;
                    if (characterUnit.BaseCharacter.CharacterStats.Toughness != null) {
                        currencyNode.Amount *= (int)characterUnit.BaseCharacter.CharacterStats.Toughness.CurrencyMultiplier;
                    }
                }
            }
            //Debug.Log($"{gameObject.name}.LootableCharacter.GetCurrencyLoot(): setting currency rolled to true");
            currencyRolled = true;
            //Debug.Log($"{gameObject.name}.LootableCharacter.GetCurrencyLoot(): returning currency: " + currencyNode.currency.MyDisplayName + "; amount: " + currencyNode.MyAmount);
            return currencyNode;
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(interactable.gameObject.name + ".LootableCharacter.Interact()");
            uIManager.interactionWindow.CloseWindow();
            if (!characterUnit.BaseCharacter.CharacterStats.IsAlive) {
                //Debug.Log($"{gameObject.name}.LootableCharacter.Interact(): Character is dead.  Showing Loot Window on interaction");
                base.Interact(source, optionIndex);
                // keep track of currency drops for combining after
                CurrencyLootDrop droppedCurrencies = new CurrencyLootDrop(systemGameManager);

                List<LootDrop> drops = new List<LootDrop>();
                List<LootDrop> itemDrops = new List<LootDrop>();
                foreach (Interactable interactable in GetLootableTargets()) {
                    LootableCharacterComponent lootableCharacter = LootableCharacterComponent.GetLootableCharacterComponent(interactable);
                    if (lootableCharacter != null) {
                        CharacterStats characterStats = CharacterUnit.GetCharacterUnit(interactable).BaseCharacter.CharacterStats as CharacterStats;
                        if (characterStats != null && characterStats.IsAlive == false && lootableCharacter.lootHolder.LootTableStates.Count > 0) {
                            //Debug.Log("Adding drops to loot table from: " + lootableCharacter.gameObject.name);

                            // get currency loot
                            if (Props.AutomaticCurrency == true) {
                                //Debug.Log($"{gameObject.name}.LootableCharacter.Interact(): automatic currency : true");
                                CurrencyNode tmpNode = lootableCharacter.GetCurrencyLoot();
                                if (tmpNode.currency != null) {
                                    droppedCurrencies.AddCurrencyNode(lootableCharacter, tmpNode);
                                }
                            }

                            // get item loot
                            foreach (LootTable lootTable in lootableCharacter.LootHolder.LootTableStates.Keys) {
                                itemDrops.AddRange(lootableCharacter.LootHolder.LootTableStates[lootTable].GetLoot(lootTable));
                                // testing - move this outside of loop because otherwise we can subscribe multiple times to loot events, and they will never be cleared
                                //lootableCharacter.MonitorLootTable();
                            }
                            if (lootableCharacter.LootHolder.LootTableStates.Count > 0 || Props.AutomaticCurrency == true) {
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
                    //lootManager.CreatePages(drops);
                    lootManager.AddLoot(drops);
                    //Debug.Log($"{gameObject.name}.LootableCharacter.Interact(): about to open window");
                    uIManager.lootWindow.OpenWindow();
                    return true;
                } else {
                    //Debug.Log($"{gameObject.name}.LootableCharacter.drops.Count: " + drops.Count);
                    return false;
                }
            }
            return false;
        }

        public void MonitorLootTable() {
            //Debug.Log(interactable.gameObject.name + ".LootableCharacterComponent.MonitorLootTable()");
            if (!monitoringTakeLoot) {
                SystemEventManager.StartListening("OnTakeLoot", HandleTakeLoot);
                monitoringTakeLoot = true;
            }
        }

        public override void StopInteract() {
            //Debug.Log($"{gameObject.name}.LootableCharacter.StopInteract()");
            base.StopInteract();
            uIManager.lootWindow.CloseWindow();
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
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.Despawn()");

            //gameObject.SetActive(false);
            //ResetLootTableStates();
            if (CharacterUnit != null) {
                CharacterUnit.Despawn();
            }
        }

        public override int GetValidOptionCount() {
            // this was commented out.  putting it back in because bunnies are 
            //if (MyCharacterUnit != null && MyCharacterUnit.MyCharacter != null && MyCharacterUnit.MyCharacter.MyCharacterStats != null) {
            if (base.GetValidOptionCount() == 0) {
                return 0;
            }
                return (CharacterUnit.BaseCharacter.CharacterStats.IsAlive == false ? 1 : 0);
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

        /*
        public void HandleRevive() {
            ResetLootTableStates();
        }
        */

        public void ResetLootTableStates() {
            foreach (LootTable lootTable in lootHolder.LootTableStates.Keys) {
                if (lootTable != null) {
                    //lootHolder.LootTableStates[lootTable].ResetLootTableState();
                    lootManager.RemoveLootTableState(lootHolder.LootTableStates[lootTable]);
                }
            }
        }

        public override string GetSummary() {
            return "Lootable";
        }
        
    }

}