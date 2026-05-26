using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Bank Config", menuName = "AnyRPG/Interactable/BankConfig")]
    public class BankConfig : InteractableOptionConfig {

        [SerializeField]
        private BankProps interactableOptionProps = new BankProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}