using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootableCharacter : InteractableOption {

        [SerializeField]
        private LootableCharacterProps lootableCharacterProps = new LootableCharacterProps();

        public override InteractableOptionProps InteractableOptionProps { get => lootableCharacterProps; }
    }

}