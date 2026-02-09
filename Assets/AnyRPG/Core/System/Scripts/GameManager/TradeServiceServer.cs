using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class TradeServiceServer : ConfiguredClass {

        //int nextTradeSession = 1;

        /// <summary>
        /// characterId, TradeSession
        /// </summary>
        private Dictionary<int, TradeSession> tradeSessionLookup = new Dictionary<int, TradeSession>();

        // game manager references
        private PlayerManagerServer playerManagerServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public void AcceptTradeInvite(int accountId) {
            //Debug.Log($"TradeServiceServer.AcceptTradeInvite({accountId})");

            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"CharacterGroupService.AcceptTradeInvite({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            int sourceCharacterId = playerManagerServer.GetPlayerCharacterId(tradeSession.SourceAccountId);
            int targetCharacterId = playerManagerServer.GetPlayerCharacterId(tradeSession.TargetAccountId);
            networkManagerServer.AdvertiseAcceptTradeInvite(tradeSession.SourceAccountId, targetCharacterId);
            networkManagerServer.AdvertiseAcceptTradeInvite(tradeSession.TargetAccountId, sourceCharacterId);
        }

        public void DeclineTradeInvite(int accountId) {
            //Debug.Log($"CharacterGroupService.DeclineCharacterGroupInvite({accountId})");

            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"CharacterGroupService.DeclineTradeInvite({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            networkManagerServer.AdvertiseDeclineTradeInvite(tradeSession.SourceAccountId);

            tradeSessionLookup.Remove(tradeSession.TargetAccountId);
            tradeSessionLookup.Remove(tradeSession.SourceAccountId);
        }

        public void RequestBeginTrade(int accountId, int targetCharacterId) {
            int targetAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(targetCharacterId);
            if (targetAccountId == -1) {
                //Debug.Log($"TradeServiceServer.RequestInviteCharacterToGroup: account not found for character {targetCharacterId}");
                return;
            }
            TradeSession tradeSession = new TradeSession(accountId, targetAccountId);
            tradeSessionLookup.Add(accountId, tradeSession);
            tradeSessionLookup.Add(targetAccountId, tradeSession);
            int sourceCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            networkManagerServer.AdvertiseRequestBeginTrade(targetAccountId, sourceCharacterId);
        }

        public void RequestAddItemsToTradeSlot(int accountId, int buttonIndex, List<long> itemInstanceIdList) {
            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"TradeServiceServer.AcceptCharacterGroupInvite({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            tradeSession.SourceAccountConfirmed = false;
            tradeSession.TargetAccountConfirmed = false;
            if (accountId == tradeSession.SourceAccountId) {
                tradeSession.SourceTradeSlots[buttonIndex] = itemInstanceIdList;
                networkManagerServer.AdvertiseAddItemsToTargetTradeSlot(tradeSession.TargetAccountId, buttonIndex, itemInstanceIdList);
            } else if (accountId == tradeSession.TargetAccountId) {
                tradeSession.targetTradeSlots[buttonIndex] = itemInstanceIdList;
                networkManagerServer.AdvertiseAddItemsToTargetTradeSlot(tradeSession.SourceAccountId, buttonIndex, itemInstanceIdList);
            }
        }

        public void RequestAddCurrencyToTrade(int accountId, int amount) {
            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"TradeServiceServer.RequestAddCurrencyToTrade({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            tradeSession.SourceAccountConfirmed = false;
            tradeSession.TargetAccountConfirmed = false;
            if (accountId == tradeSession.SourceAccountId) {
                tradeSession.SourceCurrencyAmount = amount;
                networkManagerServer.AdvertiseAddCurrencyToTrade(tradeSession.TargetAccountId, amount);
            } else if (accountId == tradeSession.TargetAccountId) {
                tradeSession.TargetCurrencyAmount = amount;
                networkManagerServer.AdvertiseAddCurrencyToTrade(tradeSession.SourceAccountId, amount);
            }
        }

        public void RequestCancelTrade(int accountId) {
            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"CharacterGroupService.RequestCancelTrade({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            CancelTrade(tradeSession);
        }

        private void CancelTrade(TradeSession tradeSession) {
            tradeSessionLookup.Remove(tradeSession.SourceAccountId);
            tradeSessionLookup.Remove(tradeSession.TargetAccountId);
            networkManagerServer.AdvertiseCancelTrade(tradeSession.TargetAccountId);
            networkManagerServer.AdvertiseCancelTrade(tradeSession.SourceAccountId);
        }

        public void RequestConfirmTrade(int accountId) {
            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            if (accountId == tradeSession.SourceAccountId) {
                tradeSession.SourceAccountConfirmed = true;
                //networkManagerServer.AdvertiseCancelTrade(tradeSession.targetAccountId);
            } else if (accountId == tradeSession.TargetAccountId) {
                tradeSession.TargetAccountConfirmed = true;
                //networkManagerServer.AdvertiseCancelTrade(tradeSession.sourceAccountId);
            }
            if (tradeSession.SourceAccountConfirmed == true && tradeSession.TargetAccountConfirmed == true) {
                CompleteTrade(tradeSession);
            }
        }

        private void CompleteTrade(TradeSession tradeSession) {
            UnitController sourceUnitController = playerManagerServer.GetUnitControllerFromAccountId(tradeSession.SourceAccountId);
            UnitController targetUnitController = playerManagerServer.GetUnitControllerFromAccountId(tradeSession.TargetAccountId);
            if (sourceUnitController == null || targetUnitController == null) {
                CancelTrade(tradeSession);
                return;
            }

            // check that the source player has the correct currency and items in inventories
            if (tradeSession.SourceCurrencyAmount > 0
                && sourceUnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) < tradeSession.SourceCurrencyAmount) {
                CancelTrade(tradeSession);
                return;
            }
            foreach (List<long> itemInstanceIdList in tradeSession.SourceTradeSlots.Values) {
                foreach (long itemInstanceId in itemInstanceIdList) {
                    if (sourceUnitController.CharacterInventoryManager.HasItem(itemInstanceId) == false) {
                        CancelTrade(tradeSession);
                        return;
                    }
                }
            }

            // check that the target player has the correct currency and items in inventories
            if (tradeSession.TargetCurrencyAmount > 0
                && targetUnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) < tradeSession.TargetCurrencyAmount) {
                CancelTrade(tradeSession);
                return;
            }
            foreach (List<long> itemInstanceIdList in tradeSession.targetTradeSlots.Values) {
                foreach (long itemInstanceId in itemInstanceIdList) {
                    if (targetUnitController.CharacterInventoryManager.HasItem(itemInstanceId) == false) {
                        CancelTrade(tradeSession);
                        return;
                    }
                }
            }

            // check that both players have enough room in their inventories
            int sourceSlotCount = tradeSession.SourceTradeSlots.Values.Select(x => x.Count > 0).Count();
            int targetSlotCount = tradeSession.targetTradeSlots.Values.Select(x => x.Count > 0).Count();
            int targetNeeds = sourceSlotCount - targetSlotCount;
            int sourceNeeds = targetSlotCount - sourceSlotCount;
            if (targetNeeds > targetUnitController.CharacterInventoryManager.EmptySlotCount()) {
                CancelTrade(tradeSession);
                return;
            }
            if (sourceNeeds > sourceUnitController.CharacterInventoryManager.EmptySlotCount()) {
                CancelTrade(tradeSession);
                return;
            }

            // swap currencies
            sourceUnitController.CharacterCurrencyManager.SpendCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, tradeSession.SourceCurrencyAmount);
            sourceUnitController.CharacterCurrencyManager.AddCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, tradeSession.TargetCurrencyAmount);
            targetUnitController.CharacterCurrencyManager.SpendCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, tradeSession.TargetCurrencyAmount);
            targetUnitController.CharacterCurrencyManager.AddCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, tradeSession.SourceCurrencyAmount);

            // get references to instantiated items
            List<InstantiatedItem> sourceItemList = new List<InstantiatedItem>();
            List<InstantiatedItem> targetItemList = new List<InstantiatedItem>();

            foreach (List<long> itemInstanceIdList in tradeSession.SourceTradeSlots.Values) {
                foreach (long itemInstanceId in itemInstanceIdList) {
                    sourceItemList.Add(systemItemManager.GetExistingInstantiatedItem(itemInstanceId));
                }
            }

            foreach (List<long> itemInstanceIdList in tradeSession.targetTradeSlots.Values) {
                foreach (long itemInstanceId in itemInstanceIdList) {
                    targetItemList.Add(systemItemManager.GetExistingInstantiatedItem(itemInstanceId));
                }
            }

            // remove items from inventory to make space for new ones
            foreach (InstantiatedItem item in sourceItemList) {
                sourceUnitController.CharacterInventoryManager.RemoveInventoryItem(item);
            }

            foreach (InstantiatedItem item in targetItemList) {
                targetUnitController.CharacterInventoryManager.RemoveInventoryItem(item);
            }

            // add new items to inventories
            foreach (InstantiatedItem item in sourceItemList) {
                targetUnitController.CharacterInventoryManager.AddItem(item, false);
            }

            foreach (InstantiatedItem item in targetItemList) {
                sourceUnitController.CharacterInventoryManager.AddItem(item, false);
            }

            networkManagerServer.AdvertiseCompleteTrade(tradeSession.SourceAccountId);
            networkManagerServer.AdvertiseCompleteTrade(tradeSession.TargetAccountId);

            // clean up trade session lookups
            tradeSessionLookup.Remove(tradeSession.TargetAccountId);
            tradeSessionLookup.Remove(tradeSession.SourceAccountId);
        }

        public void RequestUnconfirmTrade(int accountId) {
            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            if (accountId == tradeSession.SourceAccountId) {
                tradeSession.SourceAccountConfirmed = false;
                //networkManagerServer.AdvertiseCancelTrade(tradeSession.targetAccountId);
            } else if (accountId == tradeSession.TargetAccountId) {
                tradeSession.TargetAccountConfirmed = false;
                //networkManagerServer.AdvertiseCancelTrade(tradeSession.sourceAccountId);
            }
        }
    }

    public class TradeSession {
        public int SourceAccountId = 0;
        public int SourceCurrencyAmount = 0;
        public bool SourceAccountConfirmed = false;
        public Dictionary<int, List<long>> SourceTradeSlots = new Dictionary<int, List<long>>();

        public int TargetAccountId = 0;
        public int TargetCurrencyAmount = 0;
        public bool TargetAccountConfirmed = false;
        public Dictionary<int, List<long>> targetTradeSlots = new Dictionary<int, List<long>>();

        public TradeSession(int sourceAccountId, int targetAccountId) {
            this.SourceAccountId = sourceAccountId;
            this.TargetAccountId = targetAccountId;
            for (int i = 0; i < SourceTradeSlots.Count; i++) {
                SourceTradeSlots[i] = new List<long>();
                targetTradeSlots[i] = new List<long>();
            }
        }
    }

}