using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CreditController : MonoBehaviour {

        [SerializeField]
        private Text creditNameText = null;

        [SerializeField]
        private Text attributionText = null;

        private string url = string.Empty;

        public Text MyCreditNameText { get => creditNameText; set => creditNameText = value; }
        public Text MyAttributionText { get => attributionText; set => attributionText = value; }
        public string MyUrl { get => url; set => url = value; }

        public void OpenURL() {
            Application.OpenURL(MyUrl);
        }

    }

}
