using UnityEngine;

namespace AnyRPG {
    public class LootableCharacter : InteractableOption {

        [SerializeField]
        private LootableCharacterProps lootableCharacterProps = new LootableCharacterProps();

        public override InteractableOptionProps InteractableOptionProps { get => lootableCharacterProps; }
    }

}