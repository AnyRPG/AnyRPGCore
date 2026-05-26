using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Storage Container Config", menuName = "AnyRPG/Interactable/StorageContainerConfig")]
    public class StorageContainerConfig : InteractableOptionConfig {

        [SerializeField]
        private StorageContainerProps interactableOptionProps = new StorageContainerProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}