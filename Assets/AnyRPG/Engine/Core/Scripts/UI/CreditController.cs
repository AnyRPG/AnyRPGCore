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
        private string downloadUrl = string.Empty;

        public TextMeshProUGUI MyCreditNameText { get => creditNameText; set => creditNameText = value; }
        public TextMeshProUGUI MyAttributionText { get => attributionText; set => attributionText = value; }
        public string UserUrl { get => url; set => url = value; }
        public string DownloadUrl { get => downloadUrl; set => downloadUrl = value; }

        public void OpenURL() {
            if (UserUrl != null && UserUrl != string.Empty) {
                Application.OpenURL(UserUrl);
            }
        }

        public void OpenDownloadURL() {
            if (downloadUrl != null && downloadUrl != string.Empty) {
                Application.OpenURL(downloadUrl);
            }
        }

    }

}
