using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmPanel : WindowPanel {

        [Header("Confirm Invalid Mail Recipient")]

        [SerializeField]
        private TextMeshProUGUI confirmText = null;

        // game manager references
        private UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            uIManager.OnConfirmationPopup += HandleConfirmationPopup;

        }

        private void HandleConfirmationPopup(string messageText) {
            Open();
            confirmText.text = messageText;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        public void ConfirmAction() {
            //Debug.Log("ConfirmPanel.ConfirmAction()");

            Close();
        }


    }

}