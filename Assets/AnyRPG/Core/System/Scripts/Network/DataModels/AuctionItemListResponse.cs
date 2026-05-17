using System;
using System.Collections.Generic;

namespace AnyRPG {

    public class AuctionItemListResponse {
        // intentionally camelCase for compatibility with API server serializer
        public List<AuctionItemSerializedData> auctionItems;

        public AuctionItemListResponse() {
            auctionItems = new List<AuctionItemSerializedData>();
        }
    }

}