using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Auction Config", menuName = "AnyRPG/Interactable/AuctionConfig")]
    public class AuctionConfig : InteractableOptionConfig {

        [SerializeField]
        private AuctionProps interactableOptionProps = new AuctionProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}