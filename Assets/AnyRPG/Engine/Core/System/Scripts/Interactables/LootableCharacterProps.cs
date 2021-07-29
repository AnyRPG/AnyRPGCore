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

        public override Sprite Icon { get => (SystemGameManager.Instance.SystemConfigurationManager.LootableCharacterInteractionPanelImage != null ? SystemGameManager.Instance.SystemConfigurationManager.LootableCharacterInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemGameManager.Instance.SystemConfigurationManager.LootableCharacterNamePlateImage != null ? SystemGameManager.Instance.SystemConfigurationManager.LootableCharacterNamePlateImage : base.NamePlateImage); }
        public bool AutomaticCurrency { get => automaticCurrency; set => automaticCurrency = value; }
        public List<string> LootTableNames { get => lootTableNames; set => lootTableNames = value; }

        //public List<LootTable> LootTables { get => lootTables; set => lootTables = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            //Debug.Log("InteractableOptionComponent().GetInteractableOption: (" + (interactable == null ? "null" : interactable.DisplayName) + ")");
            InteractableOptionComponent returnValue = new LootableCharacterComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        /*
        // disabled since props is shared so everything would get the same table anyway.
        // moved to component
        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            foreach (string lootTableName in lootTableNames) {
                LootTable lootTable = SystemLootTableManager.Instance.GetNewResource(lootTableName);
                if (lootTable != null) {
                    lootTables.Add(lootTable);
                }
            }
        }
        */
    }

}