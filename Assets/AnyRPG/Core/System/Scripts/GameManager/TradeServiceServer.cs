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
            int sourceCharacterId = playerManagerServer.GetPlayerCharacterId(tradeSession.sourceAccountId);
            int targetCharacterId = playerManagerServer.GetPlayerCharacterId(tradeSession.targetAccountId);
            networkManagerServer.AdvertiseAcceptTradeInvite(tradeSession.sourceAccountId, targetCharacterId);
            networkManagerServer.AdvertiseAcceptTradeInvite(tradeSession.targetAccountId, sourceCharacterId);
        }

        public void DeclineTradeInvite(int accountId) {
            //Debug.Log($"CharacterGroupService.DeclineCharacterGroupInvite({accountId})");

            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"CharacterGroupService.DeclineTradeInvite({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            networkManagerServer.AdvertiseDeclineTradeInvite(tradeSession.sourceAccountId);

            tradeSessionLookup.Remove(tradeSession.targetAccountId);
            tradeSessionLookup.Remove(tradeSession.sourceAccountId);
        }

        public void RequestBeginTrade(int accountId, int targetCharacterId) {
            int targetAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(targetCharacterId);
            if (targetAccountId == 0) {
                //Debug.Log($"TradeServiceServer.RequestInviteCharacterToGroup: account not found for character {targetCharacterId}");
                return;
            }
            TradeSession tradeSession = new TradeSession(accountId, targetAccountId);
            tradeSessionLookup.Add(accountId, tradeSession);
            tradeSessionLookup.Add(targetAccountId, tradeSession);
            int sourceCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            networkManagerServer.AdvertiseRequestBeginTrade(targetAccountId, sourceCharacterId);
        }

        public void RequestAddItemsToTradeSlot(int accountId, int buttonIndex, List<int> itemIdList) {
            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"TradeServiceServer.AcceptCharacterGroupInvite({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            tradeSession.sourceAccountConfirmed = false;
            tradeSession.targetAccountConfirmed = false;
            if (accountId == tradeSession.sourceAccountId) {
                tradeSession.sourceTradeSlots[buttonIndex] = itemIdList;
                networkManagerServer.AdvertiseAddItemsToTargetTradeSlot(tradeSession.targetAccountId, buttonIndex, itemIdList);
            } else if (accountId == tradeSession.targetAccountId) {
                tradeSession.targetTradeSlots[buttonIndex] = itemIdList;
                networkManagerServer.AdvertiseAddItemsToTargetTradeSlot(tradeSession.sourceAccountId, buttonIndex, itemIdList);
            }
        }

        public void RequestAddCurrencyToTrade(int accountId, int amount) {
            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"TradeServiceServer.RequestAddCurrencyToTrade({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            tradeSession.sourceAccountConfirmed = false;
            tradeSession.targetAccountConfirmed = false;
            if (accountId == tradeSession.sourceAccountId) {
                tradeSession.sourceCurrencyAmount = amount;
                networkManagerServer.AdvertiseAddCurrencyToTrade(tradeSession.targetAccountId, amount);
            } else if (accountId == tradeSession.targetAccountId) {
                tradeSession.targetCurrencyAmount = amount;
                networkManagerServer.AdvertiseAddCurrencyToTrade(tradeSession.sourceAccountId, amount);
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
            tradeSessionLookup.Remove(tradeSession.sourceAccountId);
            tradeSessionLookup.Remove(tradeSession.targetAccountId);
            networkManagerServer.AdvertiseCancelTrade(tradeSession.targetAccountId);
            networkManagerServer.AdvertiseCancelTrade(tradeSession.sourceAccountId);
        }

        public void RequestConfirmTrade(int accountId) {
            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            if (accountId == tradeSession.sourceAccountId) {
                tradeSession.sourceAccountConfirmed = true;
                //networkManagerServer.AdvertiseCancelTrade(tradeSession.targetAccountId);
            } else if (accountId == tradeSession.targetAccountId) {
                tradeSession.targetAccountConfirmed = true;
                //networkManagerServer.AdvertiseCancelTrade(tradeSession.sourceAccountId);
            }
            if (tradeSession.sourceAccountConfirmed == true && tradeSession.targetAccountConfirmed == true) {
                CompleteTrade(tradeSession);
            }
        }

        private void CompleteTrade(TradeSession tradeSession) {
            UnitController sourceUnitController = playerManagerServer.GetUnitControllerFromAccountId(tradeSession.sourceAccountId);
            UnitController targetUnitController = playerManagerServer.GetUnitControllerFromAccountId(tradeSession.targetAccountId);
            if (sourceUnitController == null || targetUnitController == null) {
                CancelTrade(tradeSession);
                return;
            }

            // check that the source player has the correct currency and items in inventories
            if (tradeSession.sourceCurrencyAmount > 0
                && sourceUnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) < tradeSession.sourceCurrencyAmount) {
                CancelTrade(tradeSession);
                return;
            }
            foreach (List<int> itemList in tradeSession.sourceTradeSlots.Values) {
                foreach (int itemId in itemList) {
                    if (sourceUnitController.CharacterInventoryManager.HasItem(itemId) == false) {
                        CancelTrade(tradeSession);
                        return;
                    }
                }
            }

            // check that the target player has the correct currency and items in inventories
            if (tradeSession.targetCurrencyAmount > 0
                && targetUnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) < tradeSession.targetCurrencyAmount) {
                CancelTrade(tradeSession);
                return;
            }
            foreach (List<int> itemList in tradeSession.targetTradeSlots.Values) {
                foreach (int itemId in itemList) {
                    if (targetUnitController.CharacterInventoryManager.HasItem(itemId) == false) {
                        CancelTrade(tradeSession);
                        return;
                    }
                }
            }

            // check that both players have enough room in their inventories
            int sourceSlotCount = tradeSession.sourceTradeSlots.Values.Select(x => x.Count > 0).Count();
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
            sourceUnitController.CharacterCurrencyManager.SpendCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, tradeSession.sourceCurrencyAmount);
            sourceUnitController.CharacterCurrencyManager.AddCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, tradeSession.targetCurrencyAmount);
            targetUnitController.CharacterCurrencyManager.SpendCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, tradeSession.targetCurrencyAmount);
            targetUnitController.CharacterCurrencyManager.AddCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, tradeSession.sourceCurrencyAmount);

            // get references to instantiated items
            List<InstantiatedItem> sourceItemList = new List<InstantiatedItem>();
            List<InstantiatedItem> targetItemList = new List<InstantiatedItem>();

            foreach (List<int> itemList in tradeSession.sourceTradeSlots.Values) {
                foreach (int itemId in itemList) {
                    sourceItemList.Add(systemItemManager.GetExistingInstantiatedItem(itemId));
                }
            }

            foreach (List<int> itemList in tradeSession.targetTradeSlots.Values) {
                foreach (int itemId in itemList) {
                    targetItemList.Add(systemItemManager.GetExistingInstantiatedItem(itemId));
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

            networkManagerServer.AdvertiseCompleteTrade(tradeSession.sourceAccountId);
            networkManagerServer.AdvertiseCompleteTrade(tradeSession.targetAccountId);

            // clean up trade session lookups
            tradeSessionLookup.Remove(tradeSession.targetAccountId);
            tradeSessionLookup.Remove(tradeSession.sourceAccountId);
        }

        public void RequestUnconfirmTrade(int accountId) {
            if (tradeSessionLookup.ContainsKey(accountId) == false) {
                //Debug.Log($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}) trade session not found");
                return;
            }
            TradeSession tradeSession = tradeSessionLookup[accountId];
            if (accountId == tradeSession.sourceAccountId) {
                tradeSession.sourceAccountConfirmed = false;
                //networkManagerServer.AdvertiseCancelTrade(tradeSession.targetAccountId);
            } else if (accountId == tradeSession.targetAccountId) {
                tradeSession.targetAccountConfirmed = false;
                //networkManagerServer.AdvertiseCancelTrade(tradeSession.sourceAccountId);
            }
        }
    }

    public class TradeSession {
        public int sourceAccountId = 0;
        public int sourceCurrencyAmount = 0;
        public bool sourceAccountConfirmed = false;
        public Dictionary<int, List<int>> sourceTradeSlots = new Dictionary<int, List<int>>();

        public int targetAccountId = 0;
        public int targetCurrencyAmount = 0;
        public bool targetAccountConfirmed = false;
        public Dictionary<int, List<int>> targetTradeSlots = new Dictionary<int, List<int>>();

        public TradeSession(int sourceAccountId, int targetAccountId) {
            this.sourceAccountId = sourceAccountId;
            this.targetAccountId = targetAccountId;
            for (int i = 0; i < sourceTradeSlots.Count; i++) {
                sourceTradeSlots[i] = new List<int>();
                targetTradeSlots[i] = new List<int>();
            }
        }
    }

}