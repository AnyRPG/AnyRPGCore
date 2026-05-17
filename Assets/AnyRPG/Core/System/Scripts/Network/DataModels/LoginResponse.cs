using System;

namespace AnyRPG {
    
    [Serializable]
    public class LoginResponse {
        // intentionally camelCase for compatibility with API server serializer
        public int accountId;
        public string token = string.Empty;
    }
}
