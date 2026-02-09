using UnityEngine;
using UnityEngine.Networking;

namespace AnyRPG {

    // Custom handler that skips all certificate checks
    public class BypassCertificateHandler : CertificateHandler {
        protected override bool ValidateCertificate(byte[] certificateData) {
            // Always returning true ignores any SSL certificate errors
            return true;
        }
    }
}
