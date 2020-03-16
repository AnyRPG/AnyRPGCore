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

        public string MyCreditName { get => creditName; set => creditName = value; }
        public string MyCreditAttribution { get => creditAttribution; set => creditAttribution = value; }
        public string MyUrl { get => url; set => url = value; }
    }

}