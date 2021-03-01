using AnyRPG;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace AnyRPG {
    public class CreditsController : WindowContentController {

        [SerializeField]
        private GameObject creditTemplate = null;

        [SerializeField]
        private GameObject creditCategoryTemplate = null;

        [SerializeField]
        private Transform creditsContainer = null;

        public void Start() {

            PopulateCredits();
        }

        public void PopulateCredits() {
            bool firstCategoryPassed = false;
            foreach (CreditsCategory creditsCategory in SystemCreditsCategoryManager.MyInstance.GetResourceList()) {
                GameObject go = null;
                if (firstCategoryPassed) {
                    go = ObjectPooler.MyInstance.GetPooledObject(creditCategoryTemplate, creditsContainer);
                    go.GetComponent<CreditCategoryController>().MyTitleText.text = " ";
                }
                go = ObjectPooler.MyInstance.GetPooledObject(creditCategoryTemplate, creditsContainer);
                go.GetComponent<CreditCategoryController>().MyTitleText.text = creditsCategory.DisplayName;
                firstCategoryPassed = true;
                foreach (CreditsNode creditsNode in creditsCategory.MyCreditsNodes) {
                    go = ObjectPooler.MyInstance.GetPooledObject(creditTemplate, creditsContainer);
                    CreditController creditController = go.GetComponent<CreditController>();
                    creditController.MyCreditNameText.text = creditsNode.CreditName;
                    creditController.MyAttributionText.text = creditsNode.CreditAttribution;
                    creditController.UserUrl = creditsNode.MyUrl;
                    creditController.DownloadUrl = creditsNode.DownloadUrl;
                }
            }
        }

        public void CloseMenu() {
            //SystemWindowManager.MyInstance.mainMenuWindow.OpenWindow();
            SystemWindowManager.MyInstance.creditsWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
            //UserInterfacePanel();
        }
    }
}