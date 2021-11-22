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

        [SerializeField]
        private HighlightButton nameHighlightButton = null;

        [SerializeField]
        private HighlightButton attributionHighlightButton = null;

        private string url = string.Empty;
        private string downloadUrl = string.Empty;

        public TextMeshProUGUI CreditNameText { get => creditNameText; set => creditNameText = value; }
        public TextMeshProUGUI AttributionText { get => attributionText; set => attributionText = value; }
        public string UserUrl { get => url; set => url = value; }
        public string DownloadUrl { get => downloadUrl; set => downloadUrl = value; }
        public HighlightButton NameHighlightButton { get => nameHighlightButton; }
        public HighlightButton AttributionHighlightButton { get => attributionHighlightButton; }

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
