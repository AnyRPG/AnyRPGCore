using UnityEngine;

namespace AnyRPG {
    public class StorageContainerInteractable : InteractableOption {

        [SerializeField]
        private StorageContainerProps storageContainerProps = new StorageContainerProps();

        public override InteractableOptionProps InteractableOptionProps { get => storageContainerProps; }
    }

}