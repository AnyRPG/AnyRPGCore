using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Vendor Config", menuName = "AnyRPG/Interactable/VendorConfig")]
    public class VendorConfig : InteractableOptionConfig {

        [SerializeField]
        private VendorProps interactableOptionProps = new VendorProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}