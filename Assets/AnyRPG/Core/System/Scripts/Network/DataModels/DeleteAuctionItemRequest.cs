using UnityEngine;

namespace AnyRPG {

    public class DeleteAuctionItemRequest {
        public int Id;

        public DeleteAuctionItemRequest(int auctionItemId) {
            Id = auctionItemId;
        }
    }
}
