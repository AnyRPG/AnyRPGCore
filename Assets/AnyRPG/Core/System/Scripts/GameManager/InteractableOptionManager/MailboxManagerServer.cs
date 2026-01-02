using System;
using System.Collections.Generic;

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
            if (playerCharacterId == 0) {
                return;
            }
            if (mailService.DeleteMessage(playerCharacterId, messageId) == true) {
                networkManagerServer.AdvertiseDeleteMailMessage(accountId, messageId);
            }
        }

        public void RequestTakeMailAttachment(int accountId, int messageId, int attachmentSlotId) {
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == 0) {
                return;
            }
            if (mailService.TakeAttachment(playerCharacterId, messageId, attachmentSlotId) == true) {
                networkManagerServer.AdvertiseTakeMailAttachment(accountId, messageId, attachmentSlotId);
            }
        }

        public void RequestTakeMailAttachments(int accountId, int messageId) {
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == 0) {
                return;
            }
            if (mailService.TakeAttachments(playerCharacterId, messageId) == true) {
                networkManagerServer.AdvertiseTakeMailAttachments(accountId, messageId);
            }
            
        }

        public void RequestMarkMailAsRead(int accountId, int messageId) {
            mailService.MarkMessageAsRead(accountId, messageId);
        }
    }

}