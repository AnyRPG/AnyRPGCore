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

        [Header("Loot")]

        [Tooltip("If true, when killed, this unit will drop the system defined currency amount for its level and toughness")]
        [SerializeField]
        private bool automaticCurrency = false;

        [Tooltip("Define items that can drop in this list")]
        [SerializeField]
        private List<string> lootTableNames = new List<string>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyLootableCharacterInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyLootableCharacterInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyLootableCharacterNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyLootableCharacterNamePlateImage : base.NamePlateImage); }

        public InteractableOption GetInteractableOption(Interactable interactable) {
            return new LootableCharacter(interactable, this);
        }
    }

}