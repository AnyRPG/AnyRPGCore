using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CreditsNode {

        // time in seconds to show this text
        [SerializeField]
        private string creditName = string.Empty;

        [SerializeField]
        private string creditAttribution = string.Empty;

        [SerializeField]
        private string url;

        [SerializeField]
        private string email = string.Empty;

        [SerializeField]
        private string userUrl = string.Empty;

        [SerializeField]
        private string downloadUrl = string.Empty;


        public string CreditName { get => creditName; set => creditName = value; }
        public string CreditAttribution { get => creditAttribution; set => creditAttribution = value; }
        public string Url {
            get {
                if (userUrl != null && userUrl != string.Empty) {
                    return userUrl;
                }
                return url;
            }
            set => url = value;
        }
        public string Email { get => email; set => email = value; }
        public string UserUrl { get => userUrl; set => userUrl = value; }
        public string DownloadUrl { get => downloadUrl; set => downloadUrl = value; }
    }

}