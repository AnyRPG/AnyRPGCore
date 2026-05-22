using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class StorageContainerProps : InteractableOptionProps {

        [Header("Storage Container")]

        [Tooltip("The number of item slots in the container.")]
        [SerializeField]
        private int numberOfSlots = 20;

        /*
        [Tooltip("The items that will be in the container when it is first opened.  This is only used if the container has not been opened before.  Once the container is opened, the contents are saved and this list is ignored.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        private List<string> defaultItemNames = new List<string>();
        */

        [Tooltip("The loot table that will be used to generate the contents of the container if it is empty when opened.  This is only used if the container has not been opened before.  Once the container is opened, the contents are saved and this loot table is ignored.")]
        [SerializeField]
        private ContainerLootTable containerLootTable = new ContainerLootTable();

        //private List<Item> defaultItems = new List<Item>();

        public override Sprite Icon { get => (systemConfigurationManager.StorageContainerInteractionPanelImage != null ? systemConfigurationManager.StorageContainerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.StorageContainerNamePlateImage != null ? systemConfigurationManager.StorageContainerNamePlateImage : base.NamePlateImage); }
        public int NumberOfSlots { get => numberOfSlots; }
        //public List<Item> DefaultItems { get => defaultItems; }
        public ContainerLootTable ContainerLootTable { get => containerLootTable; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new StorageContainerComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        /*
        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            foreach (string itemName in defaultItemNames) {
                if (itemName == string.Empty) {
                    Debug.LogError("StorageContainerProps.SetupScriptableObjects(): Empty item name in defaultItemNames list.  CHECK INSPECTOR");
                    continue;
                }
                Item tmpItem = systemDataFactory.GetResource<Item>(itemName);
                if (tmpItem != null) {
                    defaultItems.Add(tmpItem);
                } else {
                    Debug.LogError($"StorageContainerProps.SetupScriptableObjects(): Could not find item : {itemName} while inititalizing.  CHECK INSPECTOR");
                }

            }
        }
        */

    }

}