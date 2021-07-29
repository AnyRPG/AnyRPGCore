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

        public override void Init() {
            base.Init();
            PopulateCredits();
        }

        public void PopulateCredits() {
            bool firstCategoryPassed = false;
            foreach (CreditsCategory creditsCategory in SystemCreditsCategoryManager.Instance.GetResourceList()) {
                GameObject go = null;
                if (firstCategoryPassed) {
                    go = ObjectPooler.Instance.GetPooledObject(creditCategoryTemplate, creditsContainer);
                    go.GetComponent<CreditCategoryController>().MyTitleText.text = " ";
                }
                go = ObjectPooler.Instance.GetPooledObject(creditCategoryTemplate, creditsContainer);
                go.GetComponent<CreditCategoryController>().MyTitleText.text = creditsCategory.DisplayName;
                firstCategoryPassed = true;
                foreach (CreditsNode creditsNode in creditsCategory.MyCreditsNodes) {
                    go = ObjectPooler.Instance.GetPooledObject(creditTemplate, creditsContainer);
                    CreditController creditController = go.GetComponent<CreditController>();
                    creditController.CreditNameText.text = creditsNode.CreditName;
                    creditController.AttributionText.text = creditsNode.CreditAttribution;
                    creditController.UserUrl = creditsNode.MyUrl;
                    creditController.DownloadUrl = creditsNode.DownloadUrl;
                }
            }
        }

        public void CloseMenu() {
            //SystemGameManager.Instance.UIManager.SystemWindowManager.mainMenuWindow.OpenWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.creditsWindow.CloseWindow();
        }

    }
}