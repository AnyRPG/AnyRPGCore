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
                    go = Instantiate(creditCategoryTemplate, creditsContainer);
                    go.GetComponent<CreditCategoryController>().MyTitleText.text = " ";
                }
                go = Instantiate(creditCategoryTemplate, creditsContainer);
                go.GetComponent<CreditCategoryController>().MyTitleText.text = creditsCategory.MyDisplayName;
                firstCategoryPassed = true;
                foreach (CreditsNode creditsNode in creditsCategory.MyCreditsNodes) {
                    go = Instantiate(creditTemplate, creditsContainer);
                    CreditController creditController = go.GetComponent<CreditController>();
                    creditController.MyCreditNameText.text = creditsNode.MyCreditName;
                    creditController.MyAttributionText.text = creditsNode.MyCreditAttribution;
                    creditController.MyUrl = creditsNode.MyUrl;
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