using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Mailbox Config", menuName = "AnyRPG/Interactable/MailboxConfig")]
    public class MailboxConfig : InteractableOptionConfig {

        [SerializeField]
        private MailboxProps interactableOptionProps = new MailboxProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}