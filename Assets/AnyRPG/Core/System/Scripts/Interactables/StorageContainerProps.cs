using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class StorageContainerProps : InteractableOptionProps {

        [Header("Storage Container")]

        [SerializeField]
        private int numberOfSlots = 20;

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        private List<string> defaultItemNames = new List<string>();

        private List<Item> defaultItems = new List<Item>();

        public override Sprite Icon { get => (systemConfigurationManager.StorageContainerInteractionPanelImage != null ? systemConfigurationManager.StorageContainerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.StorageContainerNamePlateImage != null ? systemConfigurationManager.StorageContainerNamePlateImage : base.NamePlateImage); }
        public int NumberOfSlots { get => numberOfSlots; }
        public List<Item> DefaultItems { get => defaultItems; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new StorageContainerComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

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

    }

}