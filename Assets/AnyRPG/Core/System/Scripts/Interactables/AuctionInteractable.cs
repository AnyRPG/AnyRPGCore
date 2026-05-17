using UnityEngine;

namespace AnyRPG {
    public class AuctionInteractable : InteractableOption {

        [SerializeField]
        private AuctionProps auctionProps = new AuctionProps();

        public override InteractableOptionProps InteractableOptionProps { get => auctionProps; }
    }

}