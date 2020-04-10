using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CreditController : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI creditNameText = null;

        [SerializeField]
        private TextMeshProUGUI attributionText = null;

        private string url = string.Empty;

        public TextMeshProUGUI MyCreditNameText { get => creditNameText; set => creditNameText = value; }
        public TextMeshProUGUI MyAttributionText { get => attributionText; set => attributionText = value; }
        public string MyUrl { get => url; set => url = value; }

        public void OpenURL() {
            Application.OpenURL(MyUrl);
        }

    }

}
