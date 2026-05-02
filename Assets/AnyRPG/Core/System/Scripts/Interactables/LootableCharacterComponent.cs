using AnyRPG;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootableCharacterComponent : InteractableOptionComponent, ILootHolder {

        public event System.Action<UnitController> OnLootComplete = delegate { };

        public LootableCharacterProps Props { get => interactableOptionProps as LootableCharacterProps; }

        private CharacterUnit characterUnit = null;
        private UnitController unitController = null;

        // keep track of whether or not currency for this character has been rolled.
        private bool currencyRolled = false;
        private bool currencyCollected = false;

        private bool lootCalculated = false;

        private bool monitoringTakeLoot = false;

        // hold the rolled currency amount
        //private CurrencyNode currencyNode;

        private LootTable currencyLootTable = null;
        private LootHolder lootHolder = new LootHolder();

        // account id, loot drop ids
        Dictionary<int, LootDropIdList> lootDropIdLookup = new Dictionary<int, LootDropIdList>();

        // game manager references
        private SystemAbilityController systemAbilityController = null;
        private LootManager lootManager = null;

        public CharacterUnit CharacterUnit { get => characterUnit; set => characterUnit = value; }
        public bool CurrencyRolled { get => currencyRolled; }
        //public CurrencyNode CurrencyNode { get => currencyNode; set => currencyNode = value; }
        public bool CurrencyCollected { get => currencyCollected; set => currencyCollected = value; }
        public LootHolder LootHolder { get => lootHolder; set => lootHolder = value; }

        public LootableCharacterComponent(Interactable interactable, LootableCharacterProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.Constructor()");

            CreateLootTables();
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = "Loot";
            }
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            lootHolder.Configure(systemGameManager);
            lootHolder.OnRemoveDroppedItem += HandleRemoveDroppedItem;
            lootHolder.OnInitializeItem += HandleInitializeItem;
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
            if (characterUnit != null) {
                unitController = characterUnit.UnitController;
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                    unitController.UnitEventController.OnBeforeDie += HandleBeforeDie;
                }
                if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false) {
                    // network client only
                    interactable.InteractableEventController.OnDropLoot += HandleDropLoot;
                    interactable.InteractableEventController.OnRemoveDroppedItem += HandleRemoveDroppedItemClient;
                }
            }
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.LootableCharacter.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            if (characterUnit?.UnitController.CharacterStats != null) {
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                    characterUnit.UnitController.UnitEventController.OnBeforeDie -= HandleBeforeDie;
                }
                if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false) {
                    // network client only
                    interactable.InteractableEventController.OnDropLoot -= HandleDropLoot;
                    interactable.InteractableEventController.OnRemoveDroppedItem -= HandleRemoveDroppedItemClient;
                }
            }
            if (monitoringTakeLoot) {
                //systemEventManager.OnTakeLoot -= HandleTakeLoot;
                monitoringTakeLoot = false;
            }
        }

        public override void SetSaveData(InteractableSaveData interactableSaveData) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.SetSaveData()");

            base.SetSaveData(interactableSaveData);
            LootableCharacterSaveData lootableCharacterSaveData = new LootableCharacterSaveData();
            lootableCharacterSaveData.LootDropIds = lootDropIdLookup.Values.ToList();
            foreach (LootDropIdList lootDropIdList in lootableCharacterSaveData.LootDropIds) {
                foreach (int lootDropId in lootDropIdList.LootDropIds) {
                    //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.SetSaveData(): adding loot drop id to save data: {lootDropId}");
                    lootableCharacterSaveData.LootDropSerializedDataList.Add(lootManager.GetSerializedDataForLootDropId(lootDropId));
                }
            }
            if (interactableSaveData.LootableCharacterSaveData.Count > 0) {
                interactableSaveData.LootableCharacterSaveData[0] = lootableCharacterSaveData;
            } else {
                interactableSaveData.LootableCharacterSaveData.Add(lootableCharacterSaveData);
            }
        }

        public override void LoadFromSaveData(InteractableSaveData interactableSaveData) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.LoadFromSaveData()");

            base.LoadFromSaveData(interactableSaveData);

            if (interactableSaveData.LootableCharacterSaveData.Count == 0) {
                return;
            }

            // first, add the loot drops
            foreach (LootDropSerializedData lootDropSerializedData in interactableSaveData.LootableCharacterSaveData[0].LootDropSerializedDataList) {
                lootManager.AddNetworkLootDrop(lootDropSerializedData);
            }

            // next, load the loot drop id lookups
            lootDropIdLookup = new Dictionary<int, LootDropIdList>();
            if (interactableSaveData.LootableCharacterSaveData[0].LootDropIds != null) {
                foreach (LootDropIdList lootDropList in interactableSaveData.LootableCharacterSaveData[0].LootDropIds) {
                    lootDropIdLookup.Add(lootDropList.AccountId, lootDropList);
                }
            }
        }

        private void HandleDropLoot(Dictionary<int, LootDropIdList> lootDropIdLookup) {
            this.lootDropIdLookup = lootDropIdLookup;
        }

        /*
        public void HandleTakeLoot(int accountId) {
            TryToDespawn();
        }
        */

        public void CreateLootTables() {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.CreateLootTables()");

            if (Props.AutomaticCurrency == true) {
                currencyLootTable = ScriptableObject.CreateInstance<LootTable>();
                LootGroup lootGroup = new LootGroup();
                lootGroup.GuaranteedDrop = true;
                Loot loot = new Loot();
                loot.DropChance = 100f;
                loot.Item = lootManager.CurrencyLootItem;
                lootGroup.Loot.Add(loot);
                currencyLootTable.LootGroups.Add(lootGroup);
                currencyLootTable.SetupScriptableObjects(systemGameManager);
                LootHolder.AddLootTableState(currencyLootTable);
            }
            foreach (string lootTableName in Props.LootTableNames) {
                LootTable lootTable = systemDataFactory.GetResource<LootTable>(lootTableName);
                if (lootTable != null) {
                    lootHolder.AddLootTableState(lootTable);
                }
            }
        }

        public void ClearLootTables() {
            //Debug.Log(interactable.gameObject.name + "LootableCharacterComponent.ClearLootTables()");
            ResetLootTableStates();
            lootHolder.ClearLootTableStates();
        }

        public void HandleBeforeDie(UnitController sourceUnitController) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacter.HandleBeforeDie({sourceUnitController.gameObject.name})");

            int lootCount = 0;

            if (LootHolder.LootTableStates.Count > 0) {
                foreach (AggroNode aggroNode in characterUnit.UnitController.CharacterCombat.AggroTable.AggroNodes) {
                    // only drop loot for the aggro target if it is a player
                    if (playerManagerServer.ActiveUnitControllerLookup.ContainsKey(aggroNode.aggroTarget)) {
                        //lootCount += GetLootCount(aggroNode.aggroTarget.UnitController);
                        List <LootDrop> lootDrops = DropLoot(aggroNode.aggroTarget);
                        // turn the list of lootDrops into a new list of lootDropIds
                        List<int> lootDropIds = new List<int>();
                        foreach (LootDrop lootDrop in lootDrops) {
                            lootDropIds.Add(lootDrop.LootDropId);
                        }
                        LootDropIdList lootDropList = new LootDropIdList(playerManagerServer.ActiveUnitControllerLookup[aggroNode.aggroTarget], lootDropIds);
                        lootDropIdLookup.Add(lootDropList.AccountId, lootDropList);
                        lootCount += lootDropIds.Count;
                    }
                }
            }
            lootCalculated = true;
            if (lootCount > 0 && systemConfigurationManager.LootSparkleEffect != null) {
                interactable.InteractableEventController.NotifyOnDropLoot(lootDropIdLookup);
                //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.HandleBeforeDie(): casting loot sparkle effect; loot count = {lootCount}");
                systemConfigurationManager.LootSparkleEffect.AbilityEffectProperties.Cast(systemAbilityController, interactable, interactable, new AbilityEffectContext());
            }
            TryToDespawn();
        }

        public void TryToDespawn() {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.TryToDespawn()");

            if (characterUnit.UnitController.CharacterStats.IsAlive == true) {
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
            int lootCount = GetExistingLootCount();
            if (lootCount == 0) {
                //Debug.Log($"{gameObject.name}.LootableCharacter.TryToDespawn(): loot table had no dropped items, despawning");
                if (monitoringTakeLoot) {
                    //systemEventManager.OnTakeLoot -= HandleTakeLoot;
                    monitoringTakeLoot = false;
                }

                // cancel loot sparkle here because despawn takes a while
                if (characterUnit.UnitController.CharacterStats.StatusEffects.ContainsKey(systemConfigurationManager.LootSparkleEffect.ResourceName)) {
                    characterUnit.UnitController.CharacterStats.StatusEffects[systemConfigurationManager.LootSparkleEffect.ResourceName].CancelStatusEffect();
                }
                AdvertiseLootComplete();
                Despawn();
            }
            //HandlePrerequisiteUpdates();
            HandleOptionStateChange();
        }

        public void AdvertiseLootComplete() {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.AdvertiseLootComplete()");

            if (interactable != null) {
                UnitController unitController = interactable.gameObject.GetComponent<UnitController>();
                if (unitController != null) {
                    OnLootComplete(unitController);
                }
            }
        }

        public override bool CanInteract(UnitController sourceUnitController, bool processRangeCheck, bool passedRangeCheck, bool processNonCombatCheck, bool viaSwitch = false) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.CanInteract({sourceUnitController.gameObject.name}, {processRangeCheck}, {passedRangeCheck}, {processNonCombatCheck}, {viaSwitch})");

            float factionValue = Faction.RelationWith(sourceUnitController, unitController);
            // you can't loot friendly characters
            if (factionValue > -1f ) {
                return false;
            }
            // testing, this could include a a range check, and we want that to be processed, so send in a fake faction value
            if (base.CanInteract(sourceUnitController, processRangeCheck, passedRangeCheck, false) == false || GetCurrentOptionCount(sourceUnitController) == 0) {
                return false;
            }

            int lootCount = GetLootCount(sourceUnitController);
            // changed this next line to getcurrentoptioncount to cover the size of the loot table and aliveness checks.  This should prevent an empty window from popping up after the character is looted
            if (lootHolder.LootTableStates.Count > 0 && lootCount > 0) {
                return true;
            }
            return false;
        }

        public List<LootDrop> DropLoot(UnitController sourceUnitController) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacter.DropLoot({sourceUnitController.gameObject.name})");

            List<LootDrop> droppedItems = new List<LootDrop>();

            foreach (LootTable lootTable in lootHolder.LootTableStates.Keys) {
                if (lootTable != null) {
                    //lootHolder.LootTableStates[lootTable].GetLoot(sourceUnitController, lootTable, !lootCalculated);
                    List<LootDrop> tableDroppedItems = lootHolder.GetLoot(sourceUnitController, lootTable, !lootCalculated);
                    droppedItems.AddRange(tableDroppedItems);

                    // special case for currency drop.  Maybe this could go in a PostDrop() call on the InstantiatedItems ?  Not sure that makes sense since only currencyItem would
                    // make use of the call
                    // This is here rather than the other places GetLootCount() is called because this *should* be the first time that is called immediately after death
                    /*
                    if (lootTable == currencyLootTable) {

                        if (tableDroppedItems.Count > 0 && tableDroppedItems[0].InstantiatedItem is InstantiatedCurrencyItem) {
                            InstantiatedCurrencyItem currencyItem = tableDroppedItems[0].InstantiatedItem as InstantiatedCurrencyItem;
                            currencyNode.currency = systemConfigurationManager.KillCurrency;
                            if (characterUnit != null) {
                                currencyNode.Amount = systemConfigurationManager.KillCurrencyAmountPerLevel * characterUnit.UnitController.CharacterStats.Level;
                                if (characterUnit.UnitController.BaseCharacter.UnitToughness != null) {
                                    currencyNode.Amount *= (int)characterUnit.UnitController.BaseCharacter.UnitToughness.CurrencyMultiplier;
                                }
                            }
                            currencyItem.GainCurrencyAmount = currencyNode.Amount;
                            currencyItem.GainCurrencyName = currencyNode.currency.ResourceName;
                            //Debug.Log($"{interactable.gameObject.name}.LootableCharacter.DropLoot({sourceUnitController.gameObject.name}): gaincurrencyAmount: {currencyItem.GainCurrencyAmount}, currency: {currencyItem.GainCurrencyName}");
                        }
                    }
                    */
                    //Debug.Log($"{gameObject.name}.LootableCharacter.GetLootCount(): after loot table count: " + lootCount);
                }
            }

            //Debug.Log($"{interactable.gameObject.name}.LootableCharacter.DropLoot({sourceUnitController.gameObject.name}): returning loot count: {droppedItems.Count}");
            return droppedItems;
        }

        public int GetLootCount(UnitController sourceUnitController) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacter.GetLootCount({sourceUnitController.gameObject.name})");

            int lootCount = 0;

            int accountId = 0;
            if (playerManagerServer.ActiveUnitControllerLookup.ContainsKey(sourceUnitController)) {
                accountId = playerManagerServer.ActiveUnitControllerLookup[sourceUnitController];
            } else {
                Debug.LogWarning($"{interactable.gameObject.name}.LootableCharacter.GetLootCount({sourceUnitController.gameObject.name}): accountId not found for sourceUnitController");
            }
            if (lootDropIdLookup.ContainsKey(accountId)) {
                lootCount = lootDropIdLookup[accountId].LootDropIds.Count;
            }

            //Debug.Log($"{interactable.gameObject.name}.LootableCharacter.GetLootCount({sourceUnitController.gameObject.name}): returning loot count: {lootCount}");
            return lootCount;
        }

        public int GetExistingLootCount() {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.GetExistingLootCount()");

            int lootCount = 0;
            /*
            foreach (Dictionary<int, LootTableState> lootTableStateLookup in lootHolder.LootTableStates.Values) {
                if (lootTableStateLookup != null) {
                    foreach (LootTableState state in lootTableStateLookup.Values) {
                        lootCount += state.DroppedItems.Count;
                    }
                }
            }
            */
            foreach (KeyValuePair<int, LootDropIdList> lootDropId in lootDropIdLookup) {
                lootCount += lootDropId.Value.LootDropIds.Count;
            }
            /*
            if (Props.AutomaticCurrency == true) {
                //Debug.Log($"{gameObject.name}.LootableCharacter.Interact(): automatic currency : true");
                //CurrencyNode tmpNode = GetCurrencyLoot();
                if (currencyNode.currency != null) {
                    lootCount += 1;
                    //Debug.Log($"{gameObject.name}.LootableCharacter.GetLootCount(): after currency count: " + lootCount);
                }
            }
            */
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.GetExistingLootCount(): returning loot count: {lootCount}");
            return lootCount;
        }

        public List<Interactable> GetLootableTargets() {
            Vector3 aoeSpawnCenter = CharacterUnit.Interactable.transform.position;
            Collider[] colliders = new Collider[100];
            int validMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            interactable.PhysicsScene.OverlapSphere(aoeSpawnCenter, 15f, colliders, validMask, QueryTriggerInteraction.UseGlobal);
            //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
            List<Interactable> validTargets = new List<Interactable>();
            foreach (Collider collider in colliders) {
                if (collider == null) {
                    continue;
                }
                Interactable _interactable = collider.gameObject.GetComponent<Interactable>();
                if (_interactable != null) {
                    validTargets.Add(_interactable);
                }
            }
            return validTargets;
        }

        /*
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
                    currencyNode.Amount = systemConfigurationManager.KillCurrencyAmountPerLevel * characterUnit.UnitController.CharacterStats.Level;
                    if (characterUnit.UnitController.BaseCharacter.UnitToughness != null) {
                        currencyNode.Amount *= (int)characterUnit.UnitController.BaseCharacter.UnitToughness.CurrencyMultiplier;
                    }
                }
            }
            //Debug.Log($"{gameObject.name}.LootableCharacter.GetCurrencyLoot(): setting currency rolled to true");
            currencyRolled = true;
            //Debug.Log($"{gameObject.name}.LootableCharacter.GetCurrencyLoot(): returning currency: " + currencyNode.currency.MyDisplayName + "; amount: " + currencyNode.MyAmount);
            return currencyNode;
        }
        */

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.Interact({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            if (!characterUnit.UnitController.CharacterStats.IsAlive) {
                //Debug.Log($"{gameObject.name}.LootableCharacter.Interact(): Character is dead.  Showing Loot Window on interaction");
                base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
                // keep track of currency drops for combining after
                //CurrencyLootDrop droppedCurrencies = new CurrencyLootDrop(systemGameManager);

                List<LootDrop> currencyDrops = new List<LootDrop>();
                List<LootDrop> drops = new List<LootDrop>();
                List<LootDrop> itemDrops = new List<LootDrop>();
                
                // aoe loot
                foreach (Interactable interactable in GetLootableTargets()) {
                    LootableCharacterComponent lootableCharacter = LootableCharacterComponent.GetLootableCharacterComponent(interactable);
                    if (lootableCharacter != null) {
                        CharacterStats characterStats = CharacterUnit.GetCharacterUnit(interactable).UnitController.CharacterStats as CharacterStats;
                        if (characterStats != null && characterStats.IsAlive == false && lootableCharacter.lootHolder.LootTableStates.Count > 0) {
                            //Debug.Log("Adding drops to loot table from: " + lootableCharacter.gameObject.name);

                            // get currency loot
                            // disabled since this is now handled by currency being a drop from the special currency loot table.
                            /*
                            if (lootableCharacter.Props.AutomaticCurrency == true) {
                                //Debug.Log($"{gameObject.name}.LootableCharacter.Interact(): automatic currency : true");
                                CurrencyNode tmpNode = lootableCharacter.GetCurrencyLoot();
                                if (tmpNode.currency != null) {
                                    InstantiatedCurrencyItem currencyItem = sourceUnitController.CharacterInventoryManager.GetNewInstantiatedItem(lootManager.CurrencyLootItem) as InstantiatedCurrencyItem;
                                    currencyItem.OverrideCurrency(tmpNode.currency.ResourceName, tmpNode.Amount);
                                    //LootDrop lootDrop = new LootDrop(currencyItem, systemGameManager);
                                    // TO DO : fix this code to drop the currency item from the currency loot table
                                    //currencyDrops.Add(lootDrop);
                                    //droppedCurrencies.AddCurrencyNode(lootableCharacter, tmpNode);
                                }
                            }
                            */

                            // get item loot
                            foreach (LootTable lootTable in lootableCharacter.LootHolder.LootTableStates.Keys) {
                                itemDrops.AddRange(lootableCharacter.LootHolder.GetLoot(sourceUnitController, lootTable, true));
                                // testing - move this outside of loop because otherwise we can subscribe multiple times to loot events, and they will never be cleared
                                //lootableCharacter.MonitorLootTable();
                            }
                            if (lootableCharacter.LootHolder.LootTableStates.Count > 0/* || Props.AutomaticCurrency == true*/) {
                                lootableCharacter.MonitorLootTable();
                            }

                        }
                    }
                }

                // combine all currencies from all lootable targets in range into a single currency lootdrop, and add that as the first lootdrop
                // in the drops list so it shows up on the first page as the first item
                /*
                if (droppedCurrencies.CurrencyNodes.Count > 0) {
                    drops.Add(droppedCurrencies);
                }
                */
                //drops.AddRange(currencyDrops);
                drops.AddRange(itemDrops);

                if (drops.Count > 0) {
                    //Debug.Log($"{interactable.gameObject.name}.LootableCharacter.Interact({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex}) drop count : {drops.Count}");

                    //lootManager.CreatePages(drops);
                    lootManager.AddAvailableLoot(sourceUnitController, drops);
                    //Debug.Log($"{gameObject.name}.LootableCharacter.Interact(): about to open window");
                    return true;
                } else {
                    //Debug.Log($"{gameObject.name}.LootableCharacter.drops.Count: " + drops.Count);
                    return false;
                }
            }
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.ClientInteraction({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            uIManager.interactionWindow.CloseWindow();
            uIManager.lootWindow.OpenWindow();
        }

        public void MonitorLootTable() {
            //Debug.Log(interactable.gameObject.name + ".LootableCharacterComponent.MonitorLootTable()");
            if (!monitoringTakeLoot) {
                //systemEventManager.OnTakeLoot += HandleTakeLoot;
                monitoringTakeLoot = true;
            }
        }

        public override void StopInteract() {
            //Debug.Log($"{gameObject.name}.LootableCharacter.StopInteract()");
            base.StopInteract();
            // TO DO : FIX ME need a stopInteract that is only on the client
            //uIManager.lootWindow.CloseWindow();
            //TryToDespawn();
        }

        public void Despawn() {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.Despawn()");

            //gameObject.SetActive(false);
            //ResetLootTableStates();
            if (CharacterUnit != null) {
                CharacterUnit.UnitController.Despawn(0, true, false);
            }
        }

        public override int GetValidOptionCount(UnitController sourceUnitController) {
            // this was commented out.  putting it back in because bunnies are 
            //if (MyCharacterUnit != null && MyCharacterUnit.MyCharacter != null && MyCharacterUnit.MyCharacter.MyCharacterStats != null) {
            if (base.GetValidOptionCount(sourceUnitController) == 0) {
                return 0;
            }
                return (CharacterUnit.UnitController.CharacterStats.IsAlive == false ? 1 : 0);
            //}
            //return 0;
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            if (GetValidOptionCount(sourceUnitController) == 0) {
                return 0;
            }
            int lootCount = GetLootCount(sourceUnitController);
            return ((lootCount > 0) ? 1 : 0);
        }

        /*
        public void HandleRevive() {
            ResetLootTableStates();
        }
        */

        private void HandleRemoveDroppedItemClient(int lootDropId, int accountId) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.HandleRemoveDroppedItemClient({lootDropId}, {accountId})");

            if (lootDropIdLookup.ContainsKey(accountId)) {
                lootDropIdLookup[accountId].LootDropIds.Remove(lootDropId);
            }
        }


        private void HandleRemoveDroppedItem(LootDrop lootDrop, int accountId) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.HandleRemoveDroppedItem({lootDrop.LootDropId}, {accountId})");

            if (lootDropIdLookup.ContainsKey(accountId)) {
                lootDropIdLookup[accountId].LootDropIds.Remove(lootDrop.LootDropId);
            }
            interactable.InteractableEventController.NotifyOnRemoveDroppedItem(lootDrop, accountId);
            TryToDespawn();
        }

        private void HandleInitializeItem(InstantiatedItem item) {
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.HandleInitializeItem({item.Item.ResourceName})");
            
            if (item.Item.ResourceName != lootManager.CurrencyLootItem.ResourceName) {
                return;
            }
            InstantiatedCurrencyItem currencyItem = item as InstantiatedCurrencyItem;
            int gainCurrencyAmount = 0;
            if (characterUnit != null) {
                gainCurrencyAmount = systemConfigurationManager.KillCurrencyAmountPerLevel * characterUnit.UnitController.CharacterStats.Level;
                if (characterUnit.UnitController.BaseCharacter.UnitToughness != null) {
                    gainCurrencyAmount *= (int)characterUnit.UnitController.BaseCharacter.UnitToughness.CurrencyMultiplier;
                }
            }
            item.DisplayName = $"{gainCurrencyAmount} {systemConfigurationManager.KillCurrency.DisplayName}";
            currencyItem.OverrideCurrency(systemConfigurationManager.KillCurrency.ResourceName, gainCurrencyAmount);
            //Debug.Log($"{interactable.gameObject.name}.LootableCharacterComponent.HandleInitializeItem({item.Item.ResourceName}) name: {currencyItem.GainCurrencyName} amount: {currencyItem.GainCurrencyAmount}");
        }


        public void ResetLootTableStates() {
            foreach (Dictionary<int, LootTableState> lootTableDict in lootHolder.LootTableStates.Values) {
                foreach (LootTableState lootTableState in lootTableDict.Values) {
                    lootManager.RemoveLootTableState(lootTableState);
                }
            }
        }

        public override string GetSummary(UnitController sourceUnitController) {

            if (GetLootCount(sourceUnitController) > 0) {
                return "Lootable";
            } else {
                return "";
            }
        }
        
    }

}