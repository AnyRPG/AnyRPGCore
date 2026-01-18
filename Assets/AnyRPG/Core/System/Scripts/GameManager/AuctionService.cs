using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG { 
    public class AuctionService : ConfiguredClass {

        private int auctionIdCounter = 1;
        private string baseSaveFolderName = string.Empty;
        private const int searchResultLimit = 50;

        private Dictionary<int, AuctionItem> auctionItems = new Dictionary<int, AuctionItem>();

        private Dictionary<int, InstantiatedItem> auctionItemInstances = new Dictionary<int, InstantiatedItem>();

        // game manager references
        private PlayerManagerServer playerManagerServer = null;
        private LootManager lootManager = null;
        private MessageLogServer messageLogServer = null;
        private MailService mailService = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            MakeBaseSaveFolder();
            //networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
            lootManager = systemGameManager.LootManager;
            messageLogServer = systemGameManager.MessageLogServer;
            mailService = systemGameManager.MailService;
        }

        private void HandleStopServer() {
            ClearAuctionItemMap();
        }

        public void ProcessStartServer() {
            LoadAuctionItemMap();
        }

        private void ClearAuctionItemMap() {
            //Debug.Log("PlayerCharacterService.ClearPlayerNameMap()");
            auctionItems.Clear();
        }

        private void LoadAuctionItemMap() {
            //Debug.Log("AuctionService.LoadAuctionItemMap()");

            if (Directory.Exists(baseSaveFolderName)) {
                string[] accountDirectories = Directory.GetDirectories(baseSaveFolderName);
                foreach (string playerCharacterDirectory in accountDirectories) {
                    string[] fileEntries = Directory.GetFiles(playerCharacterDirectory);
                    foreach (string fileName in fileEntries) {
                        if (fileName.EndsWith(".json")) {
                            string jsonString = File.ReadAllText(fileName);
                            AuctionItem auctionItemSaveData = JsonUtility.FromJson<AuctionItem>(jsonString);
                            //Debug.Log($"AuctionService.LoadAuctionItemMap(): Loaded auction item id {auctionItemSaveData.AuctionItemId} from file {fileName}");
                            if (!auctionItems.ContainsKey(auctionItemSaveData.AuctionItemId)) {
                                auctionItems.Add(auctionItemSaveData.AuctionItemId, auctionItemSaveData);
                                auctionItemInstances.Add(auctionItemSaveData.AuctionItemId, systemItemManager.GetExistingInstantiatedItem(auctionItemSaveData.ItemIds[0]));
                            } else {
                                Debug.LogWarning($"PlayerCharacterService.LoadPlayerNameMap(): Duplicate auction item id ({auctionItemSaveData.AuctionItemId}) found . This item will be skipped.");
                            }
                        }
                    }
                }
            }
        }

        public void LoadAuctionIdCounter(int newCounterValue) {
            //Debug.Log($"AuctionService.LoadAuctionIdCounter({newCounterValue})");

            auctionIdCounter = newCounterValue;
        }

        private void MakeBaseSaveFolder() {
            //Debug.Log("PlayerCharacterService.MakeSaveFolder()");

            Regex regex = new Regex("[^a-zA-Z0-9]");
            string gameNameString = regex.Replace(systemConfigurationManager.GameName, "");
            if (gameNameString == string.Empty) {
                return;
            }
            baseSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/Auction";
            if (!Directory.Exists($"{Application.persistentDataPath}/{gameNameString}")) {
                Directory.CreateDirectory($"{Application.persistentDataPath}/{gameNameString}");
            }
            if (!Directory.Exists($"{Application.persistentDataPath}/{gameNameString}/Online")) {
                Directory.CreateDirectory($"{Application.persistentDataPath}/{gameNameString}/Online");
            }
            if (!Directory.Exists(baseSaveFolderName)) {
                Directory.CreateDirectory(baseSaveFolderName);
            }
        }

        private void MakeAuctionItemSaveFolder(int playerCharacterId) {
            //Debug.Log("AuctionService.MakeAuctionItemSaveFolder()");

            string saveFolderName = GetAuctionItemSaveFolder(playerCharacterId);
            if (!Directory.Exists(saveFolderName)) {
                Directory.CreateDirectory(saveFolderName);
            }
        }

        private string GetAuctionItemSaveFolder(int playerCharacterId) {
            //Debug.Log("AuctionService.GetAuctionItemSaveFolder()");

            return $"{baseSaveFolderName}/{playerCharacterId}";
        }

        public bool ListNewItems(UnitController sourceUnitController, ListAuctionItemRequest listAuctionItemRequest) {
            //Debug.Log($"AuctionService.ListNewItems({sourceUnitController.gameObject.name}, {listAuctionItemRequest.ItemIds.Count})");

            int accountId = playerManagerServer.GetAccountIdFromUnitController(sourceUnitController);
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == 0) {
                return false;
            }

            if (listAuctionItemRequest.CurrencyAmount == 0) {
                networkManagerServer.AdvertiseConfirmationPopup(accountId, $"Auction must have a price");
                return false;
            }

            MakeAuctionItemSaveFolder(playerCharacterId);

            // remove duplicate itemIds from attachment slots
            List<int> uniqueIds = new List<int>();
            foreach (int itemId in listAuctionItemRequest.ItemIds) {
                if (uniqueIds.Contains(itemId) == false) {
                    uniqueIds.Add(itemId);
                }
            }

            listAuctionItemRequest.ItemIds = uniqueIds;

            // set messageId and sender
            AuctionItem auctionItem = new AuctionItem(listAuctionItemRequest, playerCharacterId);
            int auctionItemId = GetNewAuctionItemId();
            auctionItem.AuctionItemId = auctionItemId;

            if (auctionItems.ContainsKey(auctionItemId)) {
                Debug.LogWarning($"AuctionService.ListNewItems(): Generated auction item id {auctionItemId} already exists. Item will not be listed.");
                return false;
            }

            // first check to ensure item exist
            foreach (int itemId in auctionItem.ItemIds) {
                if (sourceUnitController.CharacterInventoryManager.HasItem(itemId) == false) {
                    return false;
                }
            }

            // check to make sure fee exists
            if (listAuctionItemRequest.CurrencyAmount > 0
                && sourceUnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) < systemConfigurationManager.AuctionDepositAmount) {
                return false;
            }

            // remove items from inventory
            foreach (int itemId in auctionItem.ItemIds) {
                sourceUnitController.CharacterInventoryManager.RemoveInventoryItem(itemId);
            }

            // remove fee from inventory
            sourceUnitController.CharacterCurrencyManager.SpendCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, systemConfigurationManager.AuctionDepositAmount);

            SaveAuctionFile(playerCharacterId, auctionItemId, auctionItem);

            // add to in-memory map
            auctionItems.Add(auctionItemId, auctionItem);
            auctionItemInstances.Add(auctionItemId, systemItemManager.GetExistingInstantiatedItem(auctionItem.ItemIds[0]));


            networkManagerServer.AdvertiseListAuctionItems(accountId);

            return true;
        }

        public bool SaveAuctionFile(int playerCharacterId, int auctionItemId, AuctionItem auctionItem) {
            //Debug.Log($"AuctionService.SaveAuctionFile(playerCharacterId: {playerCharacterId}, {auctionItemId})");

            string jsonString = JsonUtility.ToJson(auctionItem);
            string jsonSavePath = $"{GetAuctionItemSaveFolder(playerCharacterId)}/{auctionItemId}.json";
            File.WriteAllText(jsonSavePath, jsonString);

            return true;
        }

        private int GetNewAuctionItemId() {
            //Debug.Log($"AuctionService.GetNewAuctionItemId()");

            int returnValue = auctionIdCounter;
            auctionIdCounter++;
            serverStateService.SetAuctionIdCounter(auctionIdCounter);

            //Debug.Log($"AuctionService.GetNewMessageId() return {returnValue}");
            return returnValue;
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
            buyerMailMessageRequest.Subject = $"Auction Item Purchased: {boughtItem.DisplayName} ({auctionItem.ItemIds.Count})";
            buyerMailMessageRequest.AttachmentSlots.Add(new MailAttachmentSlot() { ItemIds = auctionItem.ItemIds});
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
            sellerMailMessageRequest.Subject = $"Auction Item Sold: {boughtItem.DisplayName} ({auctionItem.ItemIds.Count})";
            // body with breakdown of sale and fees
            string sellString = systemGameManager.CurrencyConverter.GetCombinedPriceString(new KeyValuePair<Currency, int>(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, auctionItem.CurrencyAmount));
            string feeString = systemGameManager.CurrencyConverter.GetCombinedPriceString(new KeyValuePair<Currency, int>(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, auctionFeeAmount));
            string depositString = systemGameManager.CurrencyConverter.GetCombinedPriceString(new KeyValuePair<Currency, int>(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, systemConfigurationManager.AuctionDepositAmount));
            string totalString = systemGameManager.CurrencyConverter.GetCombinedPriceString(new KeyValuePair<Currency, int>(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, currencyAfterFee));
            sellerMailMessageRequest.Body = $"Your auction for {soldItem.DisplayName} ({auctionItem.ItemIds.Count}) has been sold!\n\n" +
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
            List<int> returnedItemIds = new List<int>(auctionItem.ItemIds);
            sellerMailMessageRequest.Subject = $"Auction Canceled: {auctionItemInstances[auctionItemId].Item.DisplayName} ({returnedItemIds.Count})";
            sellerMailMessageRequest.AttachmentSlots.Add(new MailAttachmentSlot() { ItemIds = returnedItemIds });

            DeleteAuctionItem(auctionItem);

            // send the mail
            mailService.SaveMailMessage(playerCharacterId, sellerMailMessageRequest, "Auction");

            return true;
        }

        private void DeleteAuctionItem(AuctionItem auctionItem) {
            //Debug.Log($"AuctionService.DeleteAuctionItem({auctionItem.AuctionItemId})");

            auctionItems.Remove(auctionItem.AuctionItemId);
            auctionItemInstances.Remove(auctionItem.AuctionItemId);
            string jsonSavePath = $"{GetAuctionItemSaveFolder(auctionItem.SellerPlayerCharacterId)}/{auctionItem.AuctionItemId}.json";
            if (File.Exists(jsonSavePath)) {
                File.Delete(jsonSavePath);
            }
        }

        public void SearchAuctionItems(UnitController sourceUnitController, string searchstring, bool ownItems) {
            //Debug.Log($"AuctionService.SearchAuctionItems({sourceUnitController.gameObject.name}, {searchstring}, {ownItems})");

            int accountId = playerManagerServer.GetAccountIdFromUnitController(sourceUnitController);
            if (accountId == 0) {
                return;
            }
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == 0) {
                return;
            }

            AuctionItemListResponse auctionItemListResponse = GetAuctionItems(playerCharacterId, searchstring, ownItems);
            
            networkManagerServer.AdvertiseAuctionItems(accountId, auctionItemListResponse);
        }

        public AuctionItemListResponse GetAuctionItems(int playerCharacterId, string searchstring, bool ownItems) {
            //Debug.Log($"AuctionService.GetAuctionItems({playerCharacterId}, {searchstring}, {ownItems})");

            AuctionItemListResponse auctionItemListResponse = new AuctionItemListResponse();
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