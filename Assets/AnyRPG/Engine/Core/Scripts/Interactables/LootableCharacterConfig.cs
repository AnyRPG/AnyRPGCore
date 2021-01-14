using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Lootable Character Config", menuName = "AnyRPG/Interactable/LootableCharacterConfig")]
    [System.Serializable]
    public class LootableCharacterConfig : InteractableOptionConfig {

        [SerializeField]
        private LootableCharacterProps interactableOptionProps = new LootableCharacterProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}