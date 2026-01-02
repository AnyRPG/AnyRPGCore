using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG { 
    public class MailService : ConfiguredClass {

        private int mailIdCounter = 1;
        private string baseSaveFolderName = string.Empty;

        // game manager references
        private PlayerManagerServer playerManagerServer = null;
        private LootManager lootManager = null;
        private MessageLogServer messageLogServer = null;

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
        }

        private void HandleStopServer() {
            //ClearPlayerNameMap();
        }

        public void ProcessStartServer() {
            //LoadPlayerNameMap();
        }

        public void LoadMailIdCounter(int newCounterValue) {
            //Debug.Log($"MailService.LoadMailIdCounter({newCounterValue})");

            mailIdCounter = newCounterValue;
        }

        private void MakeBaseSaveFolder() {
            //Debug.Log("PlayerCharacterService.MakeSaveFolder()");

            Regex regex = new Regex("[^a-zA-Z0-9]");
            string gameNameString = regex.Replace(systemConfigurationManager.GameName, "");
            if (gameNameString == string.Empty) {
                return;
            }
            baseSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/Mail";
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

        private void MakeMessageSaveFolder(int playerCharacterId) {
            //Debug.Log("PlayerCharacterService.MakeSaveFolder()");

            string saveFolderName = GetMessageSaveFolder(playerCharacterId);
            if (!Directory.Exists(saveFolderName)) {
                Directory.CreateDirectory(saveFolderName);
            }
        }

        private string GetMessageSaveFolder(int playerCharacterId) {
            
            return $"{baseSaveFolderName}/{playerCharacterId}";
        }

        public bool SendMailMessage(UnitController sourceUnitController, MailMessageRequest mailMessageRequest) {

            int senderAccountId = playerManagerServer.GetAccountIdFromUnitController(sourceUnitController);

            int playerCharacterId = playerCharacterService.GetPlayerIdFromName(mailMessageRequest.Recipient);
            if (playerCharacterId == 0) {
                networkManagerServer.AdvertiseConfirmationPopup(senderAccountId, $"{mailMessageRequest.Recipient} is not a valid player name");
                return false;
            }

            if (mailMessageRequest.Subject == string.Empty) {
                networkManagerServer.AdvertiseConfirmationPopup(senderAccountId, $"Mail must have a subject");
                return false;
            }

            // remove duplicate itemIds from attachment slots
            List<int> uniqueIds = new List<int>();
            foreach (MailAttachmentSlot mailAttachmentSlot in mailMessageRequest.AttachmentSlots) {
                List<int> newItemIds = new List<int>();
                foreach (int itemId in mailAttachmentSlot.ItemIds) {
                    if (uniqueIds.Contains(itemId) == false) {
                        uniqueIds.Add(itemId);
                        newItemIds.Add(itemId);
                    }
                }
                mailAttachmentSlot.ItemIds = newItemIds;
            }


            // first check to ensure item exist
            foreach (MailAttachmentSlot mailAttachmentSlot in mailMessageRequest.AttachmentSlots) {
                foreach (int itemId in mailAttachmentSlot.ItemIds) {
                    if (sourceUnitController.CharacterInventoryManager.HasItem(itemId) == false) {
                        return false;
                    }
                }
            }

            // calculate postage
            int postageCurrencyAmount = systemConfigurationManager.BasePostageCurrencyAmount;
            foreach (MailAttachmentSlot mailAttachmentSlot in mailMessageRequest.AttachmentSlots) {
                if (mailAttachmentSlot.ItemIds.Count > 0) {
                    postageCurrencyAmount += systemConfigurationManager.PostageCurrencyAmountPerAttachment;
                }
            }

            // check to make sure currency and postage exist
            if (mailMessageRequest.CurrencyAmount > 0
                && sourceUnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) < (mailMessageRequest.CurrencyAmount + postageCurrencyAmount)) {
                return false;
            }


            // remove items from inventory
            foreach (MailAttachmentSlot mailAttachmentSlot in mailMessageRequest.AttachmentSlots) {
                foreach (int itemId in mailAttachmentSlot.ItemIds) {
                    sourceUnitController.CharacterInventoryManager.RemoveInventoryItem(itemId);
                }
            }

            // remove currency from inventory
            sourceUnitController.CharacterCurrencyManager.SpendCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, mailMessageRequest.CurrencyAmount);
            
            // remove postage from inventory
            sourceUnitController.CharacterCurrencyManager.SpendCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, postageCurrencyAmount);

            SaveMailMessage(playerCharacterId, mailMessageRequest, sourceUnitController.DisplayName);

            // notify source that mail was sent
            sourceUnitController.UnitEventController.NotifyOnWriteMessageFeedMessage($"Your message to {mailMessageRequest.Recipient} was sent");
            networkManagerServer.AdvertiseMailSend(senderAccountId);

            return true;
        }

        public void SaveMailMessage(int recipientPlayerCharacterId, MailMessageRequest mailMessageRequest, string senderName) {

            MakeMessageSaveFolder(recipientPlayerCharacterId);

            // set messageId and sender
            MailMessage mailMessage = new MailMessage(mailMessageRequest);
            int messageId = GetNewMessageId();
            mailMessage.MessageId = messageId;
            mailMessage.Sender = senderName;

            // add currency item
            if (mailMessageRequest.CurrencyAmount > 0) {
                InstantiatedCurrencyItem instantiatedCurrencyItem = systemItemManager.GetNewInstantiatedItem(lootManager.CurrencyLootItem) as InstantiatedCurrencyItem;
                instantiatedCurrencyItem.OverrideCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, mailMessageRequest.CurrencyAmount);
                systemItemManager.SaveDataFile(instantiatedCurrencyItem);
                MailAttachmentSlot mailAttachmentSlot = new MailAttachmentSlot();
                mailAttachmentSlot.ItemIds.Add(instantiatedCurrencyItem.InstanceId);
                mailMessage.AttachmentSlots.Add(mailAttachmentSlot);
            }

            SaveMailFile(recipientPlayerCharacterId, messageId, mailMessage);

            // notify source and target that mail was sent
            UnitController targetUnitController = playerManagerServer.GetUnitControllerFromPlayerCharacterId(recipientPlayerCharacterId);
            if (targetUnitController != null) {
                messageLogServer.WriteSystemMessage(targetUnitController, $"You have new mail from {mailMessage.Sender}.");
            }
            SendMailMessages(recipientPlayerCharacterId);
        }

        public bool SaveMailFile(int playerCharacterId, int messageId, MailMessage mailMessage) {
            //Debug.Log($"MailService.SaveMailFile({playerCharacterId}, {messageId})");

            string jsonString = JsonUtility.ToJson(mailMessage);
            string jsonSavePath = $"{GetMessageSaveFolder(playerCharacterId)}/{messageId}.json";
            File.WriteAllText(jsonSavePath, jsonString);

            return true;
        }

        private int GetNewMessageId() {
            //Debug.Log($"MailService.GetNewMessageId()");

            int returnValue = mailIdCounter;
            mailIdCounter++;
            serverStateService.SetMailIdCounter(mailIdCounter);

            //Debug.Log($"MailService.GetNewMessageId() return {returnValue}");
            return returnValue;
        }

        public bool DeleteMessage(int playerCharacterId, int messageId) {
            string jsonSavePath = $"{GetMessageSaveFolder(playerCharacterId)}/{messageId}.json";
            if (File.Exists(jsonSavePath)) {
                File.Delete(jsonSavePath);
            }
            return true;
        }

        public MailMessageListResponse GetMailMessages(int playerCharacterId) {
            //Debug.Log($"MailService.GetMailMessages({playerCharacterId})");

            MailMessageListResponse mailMessageListResponse = new MailMessageListResponse();
            string accountSaveFolder = GetMessageSaveFolder(playerCharacterId);
            if (Directory.Exists(accountSaveFolder)) {
                string[] fileEntries = Directory.GetFiles(accountSaveFolder);
                foreach (string fileName in fileEntries) {
                    if (fileName.EndsWith(".json")) {
                        string jsonString = File.ReadAllText(fileName);
                        MailMessage mailMessage = JsonUtility.FromJson<MailMessage>(jsonString);
                        if (mailMessage == null) {
                            Debug.LogWarning($"MailService.GetMailMessages({playerCharacterId}): Could not load mail message from file {fileName}. This message will be skipped.");
                            continue;
                        }
                        mailMessageListResponse.MailMessages.Add(mailMessage);
                    }
                }
            }
            mailMessageListResponse.BundleItems(systemItemManager);
            return mailMessageListResponse;
        }

        public MailMessage GetMailMessage(int playerCharacterId, int messageId) {
            string jsonSavePath = $"{GetMessageSaveFolder(playerCharacterId)}/{messageId}.json";
            if (File.Exists(jsonSavePath)) {
                string jsonString = File.ReadAllText(jsonSavePath);
                MailMessage mailMessage = JsonUtility.FromJson<MailMessage>(jsonString);
                return mailMessage;
            }
            return null;
        }

        public void SendMailMessages(int playerCharacterId) {
            //Debug.Log($"MailService.SendMailMessages({characterId})");

            // accountId will be 0 if player is not online, in which case we do not need to send mail messages
            int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(playerCharacterId);
            if (accountId == 0) {
                return;
            }
            MailMessageListResponse mailMessageListResponse = GetMailMessages(playerCharacterId);
            networkManagerServer.AdvertiseMailMessages(accountId, mailMessageListResponse);
        }

        public bool TakeAttachment(int playerCharacterId, int messageId, int attachmentSlotId) {
            //Debug.Log($"MailService.TakeAttachment(playerCharacterId: {playerCharacterId}, messageId: {messageId}, attachmentSlotId: {attachmentSlotId})");

            MailMessage mailMessage = GetMailMessage(playerCharacterId, messageId);
            if (mailMessage == null) {
                return false;
            }
            int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(playerCharacterId);
            UnitController unitController = playerManagerServer.GetUnitControllerFromAccountId(accountId);
            if (unitController == null) {
                return false;
            }
            if (TakeAttachment(playerCharacterId, attachmentSlotId, mailMessage, unitController) == true) {
                SaveMailFile(playerCharacterId, messageId, mailMessage);
                return true;
            }
            
            return false;
        }

        public bool TakeAttachment(int playerCharacterId, int attachmentSlotId, MailMessage mailMessage, UnitController unitController) {
            //Debug.Log($"MailService.TakeAttachment(playerCharacterId: {playerCharacterId}, attachmentSlotId: {attachmentSlotId}, {unitController.gameObject.name})");

            if (attachmentSlotId >= mailMessage.AttachmentSlots.Count) {
                return false;
            }
            while (mailMessage.AttachmentSlots[attachmentSlotId].ItemIds.Count > 0) {
                int itemId = mailMessage.AttachmentSlots[attachmentSlotId].ItemIds[0];
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemId);
                if (instantiatedItem != null) {
                    if (unitController.CharacterInventoryManager.AddItem(instantiatedItem, false) == false) {
                        // save file before return to ensure any items that were taken are removed from message
                        SaveMailFile(playerCharacterId, mailMessage.MessageId, mailMessage);
                        return false;
                    } else {
                        if (instantiatedItem is InstantiatedCurrencyItem) {
                            (instantiatedItem as InstantiatedCurrencyItem).Use(unitController);
                        }

                    }
                }
                mailMessage.AttachmentSlots[attachmentSlotId].ItemIds.Remove(itemId);
            }
            return true;
        }

        public bool TakeAttachments(int playerCharacterId, int messageId) {
            MailMessage mailMessage = GetMailMessage(playerCharacterId, messageId);
            if (mailMessage == null) {
                return false;
            }
            int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(playerCharacterId);
            UnitController unitController = playerManagerServer.GetUnitControllerFromAccountId(accountId);
            if (unitController == null) {
                return false;
            }
            for (int i = 0; i < mailMessage.AttachmentSlots.Count; i++) {
                if (TakeAttachment(playerCharacterId, i, mailMessage, unitController) == false) {
                    return false;
                }
            }
            SaveMailFile(playerCharacterId, messageId, mailMessage);
            return true;
        }

        public void MarkMessageAsRead(int accountId, int messageId) {
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            MailMessage mailMessage = GetMailMessage(playerCharacterId, messageId);
            if (mailMessage == null) {
                return;
            }
            mailMessage.IsRead = true;
            SaveMailFile(playerCharacterId, messageId, mailMessage);
        }
    }

}