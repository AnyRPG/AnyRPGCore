using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CreditsNode {

        [SerializeField]
        private string creditName = string.Empty;

        [SerializeField]
        private string creditAttribution = string.Empty;

        [SerializeField]
        private string email = string.Empty;

        [SerializeField]
        private string userUrl = string.Empty;

        [SerializeField]
        private string downloadUrl = string.Empty;


        public string CreditName { get => creditName; set => creditName = value; }
        public string CreditAttribution { get => creditAttribution; set => creditAttribution = value; }
        public string Email { get => email; set => email = value; }
        public string UserUrl { get => userUrl; set => userUrl = value; }
        public string DownloadUrl { get => downloadUrl; set => downloadUrl = value; }
    }

}