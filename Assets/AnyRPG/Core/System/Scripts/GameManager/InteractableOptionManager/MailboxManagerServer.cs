using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MailboxManagerServer : InteractableOptionManager {

        // game manager references
        private MailService mailService = null;
        private PlayerManagerServer playerManagerServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            mailService = systemGameManager.MailService;
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public void RequestSendMail(UnitController sourceUnitController, Interactable interactable, int componentIndex, MailMessageRequest sendMailRequest) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is MailboxComponent) {
                (currentInteractables[componentIndex] as MailboxComponent).SendMail(sourceUnitController, sendMailRequest);
            }
        }

        public void RequestDeleteMailMessage(int accountId, int messageId) {
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                return;
            }
            mailService.DeleteMessage(accountId, playerCharacterId, messageId);
            
        }

        public void RequestTakeMailAttachment(int accountId, int messageId, int attachmentSlotId) {
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                return;
            }
            mailService.RequestTakeAttachment(playerCharacterId, messageId, attachmentSlotId);
        }

        public void RequestTakeMailAttachments(int accountId, int messageId) {
            //Debug.Log($"MailboxManagerServer.RequestTakeMailAttachments(accountId: {accountId}, messageId: {messageId})");

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                return;
            }
            mailService.RequestTakeAttachments(accountId, playerCharacterId, messageId);
            
        }

        public void RequestMarkMailAsRead(int accountId, int messageId) {
            //Debug.Log($"MailboxManagerServer.RequestMarkMailAsRead(accountId: {accountId}, messageId: {messageId})");

            mailService.RequestMarkMessageAsRead(accountId, messageId);
        }
    }

}