using System;
using System.Collections.Generic;

namespace AnyRPG {

    [Serializable]
    public class ListAuctionItemRequest {
        public List<long> ItemInstanceIds = new List<long>();
        public int CurrencyAmount = 0;
        
        public ListAuctionItemRequest() { }
    }

}
