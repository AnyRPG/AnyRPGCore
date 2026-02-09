using UnityEngine;

namespace AnyRPG {

    public class LoadPlayerCharacterListRequest {
        public int AccountId;

        public LoadPlayerCharacterListRequest(int accountId) {
            AccountId = accountId;
        }
    }
}
