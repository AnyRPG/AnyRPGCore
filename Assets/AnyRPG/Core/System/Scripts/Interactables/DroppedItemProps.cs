using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class DroppedItemProps : InteractableOptionProps {

        //[Header("Dropped Item")]

        public List<InstantiatedItem> InstantiatedItems = new List<InstantiatedItem>();

        private GameObject spawnObject = null;

        public override Sprite Icon { get => (systemConfigurationManager.LootableCharacterInteractionPanelImage != null ? systemConfigurationManager.LootableCharacterInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.LootableCharacterNamePlateImage != null ? systemConfigurationManager.LootableCharacterNamePlateImage : base.NamePlateImage); }
        public GameObject SpawnObject { get => spawnObject; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new DroppedItemComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        /*
        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            foreach (string lootTableName in lootTableNames) {
                LootTable lootTable = systemDataFactory.GetResource<LootTable>(lootTableName);
                if (lootTable != null) {
                    lootTables.Add(lootTable);
                } else {
                    Debug.LogError($"Could not find loot table {lootTableName} while initializing Loot Node");
                }
            }
        }
        */
    }

}