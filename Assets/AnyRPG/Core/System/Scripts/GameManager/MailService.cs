using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG { 
    public class MailService : ConfiguredClass {

        // game manager references
        private PlayerManagerServer playerManagerServer = null;
        private LootManager lootManager = null;
        private MessageLogServer messageLogServer = null;
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
            serverDataService = systemGameManager.ServerDataService;
        }

        private void HandleStopServer() {
            //ClearPlayerNameMap();
        }

        /*
        public void ProcessStartServer() {
            //LoadPlayerNameMap();
        }
        */

        public bool SendMailMessage(UnitController sourceUnitController, MailMessageRequest mailMessageRequest) {

            int senderAccountId = playerManagerServer.GetAccountIdFromUnitController(sourceUnitController);

            int playerCharacterId = playerCharacterService.GetPlayerIdFromName(mailMessageRequest.Recipient);
            if (playerCharacterId == -1) {
                networkManagerServer.AdvertiseConfirmationPopup(senderAccountId, $"{mailMessageRequest.Recipient} is not a valid player name");
                return false;
            }

            if (mailMessageRequest.Subject == string.Empty) {
                networkManagerServer.AdvertiseConfirmationPopup(senderAccountId, $"Mail must have a subject");
                return false;
            }

            // remove duplicate itemIds from attachment slots
            List<long> uniqueIds = new List<long>();
            foreach (MailAttachmentSlot mailAttachmentSlot in mailMessageRequest.AttachmentSlots) {
                List<long> newItemInstanceIds = new List<long>();
                foreach (long itemInstanceId in mailAttachmentSlot.ItemInstanceIds) {
                    if (uniqueIds.Contains(itemInstanceId) == false) {
                        uniqueIds.Add(itemInstanceId);
                        newItemInstanceIds.Add(itemInstanceId);
                    }
                }
                mailAttachmentSlot.ItemInstanceIds = newItemInstanceIds;
            }


            // first check to ensure item exist
            foreach (MailAttachmentSlot mailAttachmentSlot in mailMessageRequest.AttachmentSlots) {
                foreach (long itemInstanceId in mailAttachmentSlot.ItemInstanceIds) {
                    if (sourceUnitController.CharacterInventoryManager.HasItem(itemInstanceId) == false) {
                        return false;
                    }
                }
            }

            // calculate postage
            int postageCurrencyAmount = systemConfigurationManager.BasePostageCurrencyAmount;
            foreach (MailAttachmentSlot mailAttachmentSlot in mailMessageRequest.AttachmentSlots) {
                if (mailAttachmentSlot.ItemInstanceIds.Count > 0) {
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
                foreach (long itemInstanceId in mailAttachmentSlot.ItemInstanceIds) {
                    sourceUnitController.CharacterInventoryManager.RemoveInventoryItem(itemInstanceId);
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

            // set messageId and sender
            MailMessage mailMessage = new MailMessage(mailMessageRequest);
            mailMessage.Sender = senderName;

            serverDataService.GetNewMailMessageId(mailMessage, mailMessageRequest, recipientPlayerCharacterId);
        }

        public void ProcessMailMessageIdAssigned(MailMessage mailMessage, MailMessageRequest mailMessageRequest, int recipientPlayerCharacterId) {
            //Debug.Log($"MailService.ProcessMailMessageIdAssigned(messageId: {mailMessage.MessageId})");

            // add currency item
            if (mailMessageRequest.CurrencyAmount > 0) {
                InstantiatedCurrencyItem instantiatedCurrencyItem = systemItemManager.GetNewInstantiatedItem(lootManager.CurrencyLootItem) as InstantiatedCurrencyItem;
                instantiatedCurrencyItem.OverrideCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, mailMessageRequest.CurrencyAmount);
                systemItemManager.CreateItemInstance(instantiatedCurrencyItem);
                MailAttachmentSlot mailAttachmentSlot = new MailAttachmentSlot();
                mailAttachmentSlot.ItemInstanceIds.Add(instantiatedCurrencyItem.InstanceId);
                mailMessage.AttachmentSlots.Add(mailAttachmentSlot);
            }

            SaveMailAndRefreshMessages(recipientPlayerCharacterId, mailMessage);

            // notify source and target that mail was sent
            UnitController targetUnitController = playerManagerServer.GetUnitControllerFromPlayerCharacterId(recipientPlayerCharacterId);
            if (targetUnitController != null) {
                messageLogServer.WriteSystemMessage(targetUnitController, $"You have new mail from {mailMessage.Sender}.");
            }
            //SendMailMessages(recipientPlayerCharacterId);
        }

        private void SaveMailAndRefreshMessages(int recipientPlayerCharacterId, MailMessage mailMessage) {
            int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(recipientPlayerCharacterId);
            if (accountId == -1) {
                return;
            }
            serverDataService.SaveMailAndRefreshMessages(accountId, recipientPlayerCharacterId, mailMessage);
        }

        public void SaveMail(int playerCharacterId, MailMessage mailMessage) {
            //Debug.Log($"MailService.SaveMailFile(playerCharacterId: {playerCharacterId}, mailMessageId: {mailMessage.MessageId})");

            serverDataService.SaveMailMessage(playerCharacterId, mailMessage);
        }

        public void DeleteMessage(int accountId, int playerCharacterId, int messageId) {
            serverDataService.DeleteMailMessage(accountId, playerCharacterId, messageId);
        }

        public void ProcessDeleteMessage(int accountId, int messageId) {
            networkManagerServer.AdvertiseDeleteMailMessage(accountId, messageId);
        }

        public void GetMailMessages(int accountId, int playerCharacterId) {
            //Debug.Log($"MailService.GetMailMessages(accountId: {accountId}, playerCharacterId: {playerCharacterId})");

            serverDataService.GetMailMessages(accountId, playerCharacterId);
        }

        public void SendMailMessages(int playerCharacterId) {
            //Debug.Log($"MailService.SendMailMessages(playerCharacterId: {playerCharacterId})");

            // accountId will be 0 if player is not online, in which case we do not need to send mail messages
            int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(playerCharacterId);
            if (accountId == -1) {
                return;
            }
            GetMailMessages(accountId, playerCharacterId);
        }

        public void ProcessMailMessageListResponse(int accountId, List<MailMessageSerializedData> mailMessageSerializedDatas) {

            List<MailMessage> mailMessages = new List<MailMessage>();

            foreach (MailMessageSerializedData mailMessageSerializedData in mailMessageSerializedDatas) {
                MailMessage mailMessage = JsonUtility.FromJson<MailMessage>(mailMessageSerializedData.saveData);
                // trust the database column with the ID in case of race conditions
                mailMessage.MessageId = mailMessageSerializedData.id;
                mailMessages.Add(mailMessage);
            }

            ProcessMailMessageListResponse(accountId, mailMessages);
        }

        public void ProcessMailMessageListResponse(int accountId, List <MailMessage> mailMessages) {

            MailMessageListBundle mailMessageListBundle = new MailMessageListBundle();

            foreach (MailMessage mailMessage in mailMessages) {
                mailMessageListBundle.MailMessages.Add(mailMessage);
            }
            mailMessageListBundle.BundleItems(systemItemManager);

            networkManagerServer.AdvertiseMailMessages(accountId, mailMessageListBundle);
        }

        public void RequestTakeAttachment(int playerCharacterId, int messageId, int attachmentSlotId) {
            //Debug.Log($"MailService.TakeAttachment(playerCharacterId: {playerCharacterId}, messageId: {messageId}, attachmentSlotId: {attachmentSlotId})");

            serverDataService.TakeAttachment(playerCharacterId, messageId, attachmentSlotId);
        }

        public void ProcessTakeAttachment(MailMessage mailMessage, int playerCharacterId, int attachmentSlotId) {

            if (mailMessage == null) {
                return;
            }
            int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(playerCharacterId);
            UnitController unitController = playerManagerServer.GetUnitControllerFromAccountId(accountId);
            if (unitController == null) {
                return;
            }
            if (TakeAttachment(playerCharacterId, attachmentSlotId, mailMessage, unitController) == true) {
                SaveMail(playerCharacterId, mailMessage);
            }

            networkManagerServer.AdvertiseTakeMailAttachment(accountId, mailMessage.MessageId, attachmentSlotId);
        }

        public bool TakeAttachment(int playerCharacterId, int attachmentSlotId, MailMessage mailMessage, UnitController unitController) {
            //Debug.Log($"MailService.TakeAttachment(playerCharacterId: {playerCharacterId}, attachmentSlotId: {attachmentSlotId}, {unitController.gameObject.name})");

            if (attachmentSlotId >= mailMessage.AttachmentSlots.Count) {
                return false;
            }
            while (mailMessage.AttachmentSlots[attachmentSlotId].ItemInstanceIds.Count > 0) {
                long itemInstanceId = mailMessage.AttachmentSlots[attachmentSlotId].ItemInstanceIds[0];
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                if (instantiatedItem != null) {
                    if (unitController.CharacterInventoryManager.AddItem(instantiatedItem, false) == false) {
                        // save file before return to ensure any items that were taken are removed from message
                        SaveMail(playerCharacterId, mailMessage);
                        return false;
                    } else {
                        if (instantiatedItem is InstantiatedCurrencyItem) {
                            (instantiatedItem as InstantiatedCurrencyItem).Use(unitController);
                        }

                    }
                }
                mailMessage.AttachmentSlots[attachmentSlotId].ItemInstanceIds.Remove(itemInstanceId);
            }
            return true;
        }

        public void RequestTakeAttachments(int accountId, int playerCharacterId, int messageId) {
            serverDataService.TakeAttachments(accountId, playerCharacterId, messageId);
        }
        public void ProcessTakeAttachments(MailMessage mailMessage, int playerCharacterId, int accountId) {
            if (mailMessage == null) {
                return;
            }
            UnitController unitController = playerManagerServer.GetUnitControllerFromAccountId(accountId);
            if (unitController == null) {
                return;
            }
            for (int i = 0; i < mailMessage.AttachmentSlots.Count; i++) {
                if (TakeAttachment(playerCharacterId, i, mailMessage, unitController) == false) {
                    return;
                }
            }
            SaveMail(playerCharacterId, mailMessage);

            networkManagerServer.AdvertiseTakeMailAttachments(accountId, mailMessage.MessageId);
        }

        public void RequestMarkMessageAsRead(int accountId, int messageId) {
            //Debug.Log($"MailService.RequestMarkMessageAsRead(accountId: {accountId}, messageId: {messageId})");

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            serverDataService.MarkMailMessageAsRead(messageId, playerCharacterId);
        }

        public void ProcessMarkMessageAsRead(MailMessage mailMessage, int playerCharacterId) {
            //Debug.Log($"MailService.ProcessMarkMessageAsRead(mailMessageId: {mailMessage.MessageId}, playerCharacterId: {playerCharacterId})");

            mailMessage.IsRead = true;
            SaveMail(playerCharacterId, mailMessage);
        }


    }

}