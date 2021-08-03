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

        private SystemWindowManager systemWindowManager = null;
        private ObjectPooler objectPooler = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);
            systemWindowManager = systemGameManager.UIManager.SystemWindowManager;
            objectPooler = systemGameManager.ObjectPooler;
            PopulateCredits();
        }

        public void PopulateCredits() {
            bool firstCategoryPassed = false;
            foreach (CreditsCategory creditsCategory in SystemDataFactory.Instance.GetResourceList<CreditsCategory>()) {
                GameObject go = null;
                if (firstCategoryPassed) {
                    go = objectPooler.GetPooledObject(creditCategoryTemplate, creditsContainer);
                    go.GetComponent<CreditCategoryController>().MyTitleText.text = " ";
                }
                go = objectPooler.GetPooledObject(creditCategoryTemplate, creditsContainer);
                go.GetComponent<CreditCategoryController>().MyTitleText.text = creditsCategory.DisplayName;
                firstCategoryPassed = true;
                foreach (CreditsNode creditsNode in creditsCategory.MyCreditsNodes) {
                    go = objectPooler.GetPooledObject(creditTemplate, creditsContainer);
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
            systemWindowManager.creditsWindow.CloseWindow();
        }

    }
}