using UnityEngine;

namespace AnyRPG {
    public class MailboxInteractable : InteractableOption {

        [SerializeField]
        private MailboxProps mailboxProps = new MailboxProps();

        public override InteractableOptionProps InteractableOptionProps { get => mailboxProps; }
    }

}