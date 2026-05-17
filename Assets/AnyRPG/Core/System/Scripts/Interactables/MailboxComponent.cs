using System;
using UnityEngine;

namespace AnyRPG {
    public class MailboxComponent : InteractableOptionComponent {

        // game manager references
        MailboxManagerClient mailboxManagerClient = null;
        MailService mailService = null;

        public MailboxProps Props { get => interactableOptionProps as MailboxProps; }

        public MailboxComponent(Interactable interactable, MailboxProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            mailboxManagerClient = systemGameManager.MailboxManagerClient;
            mailService = systemGameManager.MailService;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.MailboxInteractable.ProcessInteract({sourceUnitController?.gameObject.name}, {componentIndex}, {choiceIndex})");
            
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            //mailService.SendMailMessages(sourceUnitController.CharacterId);
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            mailboxManagerClient.SetProps(this, componentIndex, choiceIndex);
            uIManager.mailboxWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.mailboxWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + ".MailboxInteractable.GetCurrentOptionCount(): returning " + GetValidOptionCount());
            return GetValidOptionCount(sourceUnitController);
        }

        public void SendMail(UnitController sourceUnitController, MailMessageRequest mailMessageRequest) {
            //Debug.Log($"MailboxComponent.SendMail({sourceUnitController.gameObject.name})");

            mailService.SendMailMessage(sourceUnitController, mailMessageRequest);

            NotifyOnConfirmAction(sourceUnitController);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}

    }

}