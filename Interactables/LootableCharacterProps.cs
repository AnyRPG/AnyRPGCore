using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class LootableCharacterProps : InteractableOptionProps {

        [Header("Loot")]

        [Tooltip("If true, when killed, this unit will drop the system defined currency amount for its level and toughness")]
        [SerializeField]
        private bool automaticCurrency = false;

        [Tooltip("Lookup and use these named loot tables that can be shared among units")]
        [SerializeField]
        private List<string> lootTableNames = new List<string>();

        //private List<LootTable> lootTables = new List<LootTable>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyLootableCharacterInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyLootableCharacterInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyLootableCharacterNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyLootableCharacterNamePlateImage : base.NamePlateImage); }
        public bool AutomaticCurrency { get => automaticCurrency; set => automaticCurrency = value; }
        public List<string> LootTableNames { get => lootTableNames; set => lootTableNames = value; }

        //public List<LootTable> LootTables { get => lootTables; set => lootTables = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            //Debug.Log("InteractableOptionComponent().GetInteractableOption: (" + (interactable == null ? "null" : interactable.DisplayName) + ")");
            return new LootableCharacterComponent(interactable, this);
        }

        /*
        // disabled since props is shared so everything would get the same table anyway.
        // moved to component
        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            foreach (string lootTableName in lootTableNames) {
                LootTable lootTable = SystemLootTableManager.MyInstance.GetNewResource(lootTableName);
                if (lootTable != null) {
                    lootTables.Add(lootTable);
                }
            }
        }
        */
    }

}