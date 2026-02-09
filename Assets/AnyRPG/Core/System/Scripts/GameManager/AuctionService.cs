using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AnyRPG { 
    public class AuctionService : ConfiguredClass {

        private const int searchResultLimit = 50;

        private Dictionary<int, AuctionItem> auctionItems = new Dictionary<int, AuctionItem>();

        private Dictionary<int, InstantiatedItem> auctionItemInstances = new Dictionary<int, InstantiatedItem>();

        // game manager references
        private PlayerManagerServer playerManagerServer = null;
        private LootManager lootManager = null;
        private MessageLogServer messageLogServer = null;
        private MailService mailService = null;
        private ServerDataService serverDataService = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
            lootManager = systemGameManager.LootManager;
            messageLogServer = systemGameManager.MessageLogServer;
            mailService = systemGameManager.MailService;
            serverDataService = systemGameManager.ServerDataService;
        }

        private void HandleStopServer() {
            ClearAuctionItemMap();
        }

        private void ClearAuctionItemMap() {
            //Debug.Log("PlayerCharacterService.ClearPlayerNameMap()");
            auctionItems.Clear();
            auctionItemInstances.Clear();
        }

        public void LoadAuctionItemMap() {
            serverDataService.LoadAuctionItemMap();
        }

        public void ProcessLoadAuctionItemListResponse(List<AuctionItemSerializedData> auctionItemSerializedData) {
            //Debug.Log("AuctionService.LoadAuctionItemMap()");
            List<AuctionItem> auctionItems = new List<AuctionItem>();
            foreach (AuctionItemSerializedData auctionItemSerializedDataItem in auctionItemSerializedData) {
                AuctionItem auctionItem = JsonUtility.FromJson<AuctionItem>(auctionItemSerializedDataItem.saveData);
                if (auctionItem != null) {
                    auctionItems.Add(auctionItem);
                }
            }
            ProcessLoadAuctionItemList(auctionItems);
        }

        public void ProcessLoadAuctionItemList(List<AuctionItem> auctionItems) {
            //Debug.Log("AuctionService.LoadAuctionItemMap()");

            foreach (AuctionItem auctionItem in auctionItems) {
                if (auctionItem.ItemInstanceIds.Count == 0) {
                    Debug.LogWarning($"AuctionService.ProcessLoadAuctionItemList(): Loading auction item id {auctionItem.AuctionItemId}. Stack size is 0!");
                    continue;
                }
                if (!this.auctionItems.ContainsKey(auctionItem.AuctionItemId)) {
                    this.auctionItems.Add(auctionItem.AuctionItemId, auctionItem);
                    auctionItemInstances.Add(auctionItem.AuctionItemId, systemItemManager.GetExistingInstantiatedItem(auctionItem.ItemInstanceIds[0]));
                } else {
                    Debug.LogWarning($"AuctionService.ProcessLoadAuctionItemListResponse(): Duplicate auction item id ({auctionItem.AuctionItemId}) found . This item will be skipped.");
                }
            }
        }

        public void ListNewItems(UnitController sourceUnitController, ListAuctionItemRequest listAuctionItemRequest) {
            //Debug.Log($"AuctionService.ListNewItems({sourceUnitController.gameObject.name}, count: {listAuctionItemRequest.ItemInstanceIds.Count})");

            int accountId = playerManagerServer.GetAccountIdFromUnitController(sourceUnitController);
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                //Debug.Log($"AuctionService.ListNewItems({sourceUnitController.gameObject.name}, count: {listAuctionItemRequest.ItemInstanceIds.Count}) invalid player character id");
                return;
            }

            if (listAuctionItemRequest.CurrencyAmount == 0) {
                networkManagerServer.AdvertiseConfirmationPopup(accountId, $"Auction must have a price");
                //Debug.Log($"AuctionService.ListNewItems({sourceUnitController.gameObject.name}, count: {listAuctionItemRequest.ItemInstanceIds.Count}) must have a price");
                return;
            }

            // remove duplicate itemIds from attachment slots
            List<long> uniqueIds = new List<long>();
            foreach (long itemInstanceId in listAuctionItemRequest.ItemInstanceIds) {
                if (uniqueIds.Contains(itemInstanceId) == false) {
                    uniqueIds.Add(itemInstanceId);
                }
            }

            listAuctionItemRequest.ItemInstanceIds = uniqueIds;

            // first check to ensure item exist
            foreach (long itemInstanceId in listAuctionItemRequest.ItemInstanceIds) {
                if (sourceUnitController.CharacterInventoryManager.HasItem(itemInstanceId) == false) {
                    //Debug.Log($"AuctionService.ListNewItems({sourceUnitController.gameObject.name}, count: {listAuctionItemRequest.ItemInstanceIds.Count}) does not have item");
                    return;
                }
            }

            // check to make sure fee exists
            if (listAuctionItemRequest.CurrencyAmount > 0
                && sourceUnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) < systemConfigurationManager.AuctionDepositAmount) {
                //Debug.Log($"AuctionService.ListNewItems({sourceUnitController.gameObject.name}, count: {listAuctionItemRequest.ItemInstanceIds.Count}) does not have currency");
                return;
            }

            // remove items from inventory
            foreach (long itemInstanceId in listAuctionItemRequest.ItemInstanceIds) {
                sourceUnitController.CharacterInventoryManager.RemoveInventoryItem(itemInstanceId);
            }

            // remove fee from inventory
            sourceUnitController.CharacterCurrencyManager.SpendCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, systemConfigurationManager.AuctionDepositAmount);

            // create auction item
            AuctionItem auctionItem = new AuctionItem(listAuctionItemRequest, playerCharacterId);

            // get auction item Id
            serverDataService.GetNewAuctionItemId(auctionItem);
        }

        public void ProcessAuctionItemIdAssigned(AuctionItem auctionItem) {
            int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(auctionItem.SellerPlayerCharacterId);
            ProcessAuctionItemIdAssigned(auctionItem, accountId);
        }

        public void ProcessAuctionItemIdAssigned(AuctionItem auctionItem, int accountId) {

            if (auctionItems.ContainsKey(auctionItem.AuctionItemId)) {
                Debug.LogWarning($"AuctionService.ListNewItems(): Generated auction item id {auctionItem.AuctionItemId} already exists. Item will not be listed.");
                return;
            }

            SaveAuction(auctionItem.SellerPlayerCharacterId, auctionItem);

            // add to in-memory map
            auctionItems.Add(auctionItem.AuctionItemId, auctionItem);
            auctionItemInstances.Add(auctionItem.AuctionItemId, systemItemManager.GetExistingInstantiatedItem(auctionItem.ItemInstanceIds[0]));

            networkManagerServer.AdvertiseListAuctionItems(accountId);
        }

        private void SaveAuction(int playerCharacterId, AuctionItem auctionItem) {
            
            serverDataService.SaveAuction(playerCharacterId, auctionItem);

        }

        public bool BuyAuctionItem(int buyerPlayerCharacterId, int auctionItemId) {
            //Debug.Log($"AuctionService.BuyAuctionItem({buyerPlayerCharacterId}, {auctionItemId})");

            AuctionItem auctionItem = auctionItems[auctionItemId];
            if (auctionItem == null) {
                return false;
            }

            UnitController buyerUnitController = playerManagerServer.GetUnitControllerFromPlayerCharacterId(buyerPlayerCharacterId);
            if (buyerUnitController == null) {
                return false;
            }

            // ensure buyer has currency
            if (buyerUnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) < auctionItem.CurrencyAmount) {
                return false;
            }

            // create mail message with the item attached for the buyer
            MailMessageRequest buyerMailMessageRequest = new MailMessageRequest();
            Item boughtItem = auctionItemInstances[auctionItemId].Item;
            buyerMailMessageRequest.Subject = $"Auction Item Purchased: {boughtItem.DisplayName} ({auctionItem.ItemInstanceIds.Count})";
            buyerMailMessageRequest.AttachmentSlots.Add(new MailAttachmentSlot() { ItemInstanceIds = auctionItem.ItemInstanceIds});
            mailService.SaveMailMessage(buyerPlayerCharacterId, buyerMailMessageRequest, "Auction");

            // take currency from buyer
            buyerUnitController.CharacterCurrencyManager.SpendCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, auctionItem.CurrencyAmount);

            // determine currency after fee
            int auctionFeeAmount = Mathf.CeilToInt(auctionItem.CurrencyAmount * (systemConfigurationManager.AuctionSoldFeePercentage / 100f));
            int currencyAfterFee = auctionItem.CurrencyAmount + systemConfigurationManager.AuctionDepositAmount - Mathf.CeilToInt(auctionItem.CurrencyAmount * (systemConfigurationManager.AuctionSoldFeePercentage / 100f));

            // create mail message with the currency attached for the seller
            int sellerAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(auctionItem.SellerPlayerCharacterId);
            MailMessageRequest sellerMailMessageRequest = new MailMessageRequest();
            Item soldItem = auctionItemInstances[auctionItemId].Item;
            sellerMailMessageRequest.Subject = $"Auction Item Sold: {boughtItem.DisplayName} ({auctionItem.ItemInstanceIds.Count})";
            // body with breakdown of sale and fees
            string sellString = systemGameManager.CurrencyConverter.GetCombinedPriceString(new KeyValuePair<Currency, int>(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, auctionItem.CurrencyAmount));
            string feeString = systemGameManager.CurrencyConverter.GetCombinedPriceString(new KeyValuePair<Currency, int>(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, auctionFeeAmount));
            string depositString = systemGameManager.CurrencyConverter.GetCombinedPriceString(new KeyValuePair<Currency, int>(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, systemConfigurationManager.AuctionDepositAmount));
            string totalString = systemGameManager.CurrencyConverter.GetCombinedPriceString(new KeyValuePair<Currency, int>(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, currencyAfterFee));
            sellerMailMessageRequest.Body = $"Your auction for {soldItem.DisplayName} ({auctionItem.ItemInstanceIds.Count}) has been sold!\n\n" +
                $"Sale Price: {sellString}\n" +
                $"Deposit Returned: {depositString}\n" +
                $"Auction Fee: ({feeString})\n" +
                $"Total Received: {totalString}";
            sellerMailMessageRequest.CurrencyAmount = currencyAfterFee;
            mailService.SaveMailMessage(auctionItem.SellerPlayerCharacterId, sellerMailMessageRequest, "Auction");

            DeleteAuctionItem(auctionItem);

            return true;
        }

        public bool CancelAuction(int playerCharacterId, int auctionItemId) {
            //Debug.Log($"AuctionService.CancelAuction({playerCharacterId}, {auctionItemId})");

            if (!auctionItems.ContainsKey(auctionItemId)) {
                return false;
            }
            AuctionItem auctionItem = auctionItems[auctionItemId];
            if (auctionItem.SellerPlayerCharacterId != playerCharacterId) {
                return false;
            }

            // create mail message with the item attached for the seller
            MailMessageRequest sellerMailMessageRequest = new MailMessageRequest();
            List<long> returnedItemInstanceIds = new List<long>(auctionItem.ItemInstanceIds);
            sellerMailMessageRequest.Subject = $"Auction Canceled: {auctionItemInstances[auctionItemId].Item.DisplayName} ({returnedItemInstanceIds.Count})";
            sellerMailMessageRequest.AttachmentSlots.Add(new MailAttachmentSlot() { ItemInstanceIds = returnedItemInstanceIds });

            DeleteAuctionItem(auctionItem);

            // send the mail
            mailService.SaveMailMessage(playerCharacterId, sellerMailMessageRequest, "Auction");

            return true;
        }

        private void DeleteAuctionItem(AuctionItem auctionItem) {
            auctionItems.Remove(auctionItem.AuctionItemId);
            auctionItemInstances.Remove(auctionItem.AuctionItemId);
            serverDataService.DeleteAuctionItem(auctionItem);
        }

        public void SearchAuctionItems(UnitController sourceUnitController, string searchstring, bool ownItems) {
            //Debug.Log($"AuctionService.SearchAuctionItems({sourceUnitController.gameObject.name}, {searchstring}, {ownItems})");

            int accountId = playerManagerServer.GetAccountIdFromUnitController(sourceUnitController);
            if (accountId == -1) {
                return;
            }
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                return;
            }

            AuctionItemSearchListResult auctionItemListResponse = GetAuctionItems(playerCharacterId, searchstring, ownItems);
            
            networkManagerServer.AdvertiseAuctionItems(accountId, auctionItemListResponse);
        }

        public AuctionItemSearchListResult GetAuctionItems(int playerCharacterId, string searchstring, bool ownItems) {
            //Debug.Log($"AuctionService.GetAuctionItems({playerCharacterId}, {searchstring}, {ownItems})");

            AuctionItemSearchListResult auctionItemListResponse = new AuctionItemSearchListResult();
            foreach (KeyValuePair<int, AuctionItem> kvp in auctionItems) {
                AuctionItem auctionItem = kvp.Value;
                InstantiatedItem instantiatedItem = auctionItemInstances[kvp.Key];
                if (instantiatedItem == null) {
                    continue;
                }
                if (ownItems == true && auctionItem.SellerPlayerCharacterId != playerCharacterId) {
                    continue;
                }
                if (ownItems == false && auctionItem.SellerPlayerCharacterId == playerCharacterId) {
                    continue;
                }
                if (searchstring != string.Empty && instantiatedItem.DisplayName.IndexOf(searchstring, StringComparison.OrdinalIgnoreCase) < 0) {
                    continue;
                }
                auctionItemListResponse.AuctionItems.Add(new AuctionItemSerializedSearchResult(auctionItem, playerCharacterService.GetPlayerNameFromId(auctionItem.SellerPlayerCharacterId)));
                if (auctionItemListResponse.AuctionItems.Count >= searchResultLimit) {
                    break;
                }
            }

            auctionItemListResponse.BundleItems(systemItemManager);
            return auctionItemListResponse;
        }

    }

}