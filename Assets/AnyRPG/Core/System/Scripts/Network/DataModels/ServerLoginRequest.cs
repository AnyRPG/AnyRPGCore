using UnityEngine;

namespace AnyRPG {

    public class ServerLoginRequest {
        public string SharedSecret = string.Empty;

        public ServerLoginRequest(string sharedSecret) {
            SharedSecret = sharedSecret;
        }
    }
}
