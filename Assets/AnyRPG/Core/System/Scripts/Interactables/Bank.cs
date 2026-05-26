using UnityEngine;

namespace AnyRPG {
    public class Bank : InteractableOption {

        [SerializeField]
        private BankProps bankProps = new BankProps();

        public override InteractableOptionProps InteractableOptionProps { get => bankProps; }
    }

}