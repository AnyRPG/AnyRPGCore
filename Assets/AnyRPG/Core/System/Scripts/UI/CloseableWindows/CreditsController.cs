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

        [SerializeField]
        private HighlightButton returnButton = null;

        private UIManager uIManager = null;
        private ObjectPooler objectPooler = null;
        private SystemDataFactory systemDataFactory = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            PopulateCredits();
            returnButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            systemDataFactory = systemGameManager.SystemDataFactory;
        }


        public void PopulateCredits() {
            bool firstCategoryPassed = false;
            foreach (CreditsCategory creditsCategory in systemDataFactory.GetResourceList<CreditsCategory>()) {
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
                    creditController.UserUrl = creditsNode.Url;
                    creditController.DownloadUrl = creditsNode.DownloadUrl;
                }
            }
        }

        public void CloseMenu() {
            uIManager.creditsWindow.CloseWindow();
        }

    }
}