using System;
using System.Collections.Generic;

namespace AnyRPG {

    [Serializable]
    public class ListAuctionItemRequest {
        public List<int> ItemIds = new List<int>();
        public int CurrencyAmount = 0;
        
        public ListAuctionItemRequest() { }
    }

}
