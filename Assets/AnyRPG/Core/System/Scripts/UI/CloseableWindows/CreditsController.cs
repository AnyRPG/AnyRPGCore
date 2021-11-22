using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace AnyRPG {
    public class CreditsController : WindowContentController {

        [Header("CreditsController")]

        [SerializeField]
        protected GameObject creditTemplate = null;

        [SerializeField]
        protected GameObject creditCategoryTemplate = null;

        [SerializeField]
        protected Transform creditsContainer = null;

        [SerializeField]
        protected HighlightButton returnButton = null;

        protected Dictionary<string, List<CreditsNode>> categoriesDictionary = new Dictionary<string, List<CreditsNode>>();

        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SystemDataFactory systemDataFactory = null;

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

            // first, loope through and organize everything into categories
            // this is necessary because categories may not be unique across different content directories
            // and so need to have their content merged
            foreach (CreditsCategory creditsCategory in systemDataFactory.GetResourceList<CreditsCategory>()) {
                if (categoriesDictionary.ContainsKey(creditsCategory.CategoryName) == false) {
                    categoriesDictionary.Add(creditsCategory.CategoryName, creditsCategory.CreditsNodes);
                } else {
                    categoriesDictionary[creditsCategory.CategoryName].AddRange(creditsCategory.CreditsNodes);
                }
            }

            foreach (string categoryName in categoriesDictionary.Keys) {
                GameObject go = null;
                if (firstCategoryPassed) {
                    // add a blank line as a spacer between categories
                    go = objectPooler.GetPooledObject(creditCategoryTemplate, creditsContainer);
                    go.GetComponent<CreditCategoryController>().MyTitleText.text = " ";
                }
                go = objectPooler.GetPooledObject(creditCategoryTemplate, creditsContainer);
                go.GetComponent<CreditCategoryController>().MyTitleText.text = categoryName;
                firstCategoryPassed = true;
                foreach (CreditsNode creditsNode in categoriesDictionary[categoryName]) {
                    go = objectPooler.GetPooledObject(creditTemplate, creditsContainer);
                    CreditController creditController = go.GetComponent<CreditController>();
                    creditController.CreditNameText.text = creditsNode.CreditName;
                    creditController.AttributionText.text = creditsNode.CreditAttribution;
                    creditController.UserUrl = creditsNode.UserUrl;
                    creditController.DownloadUrl = creditsNode.DownloadUrl;
                    uINavigationControllers[1].AddActiveButton(creditController.NameHighlightButton);
                    uINavigationControllers[1].AddActiveButton(creditController.AttributionHighlightButton);
                    creditController.NameHighlightButton.Configure(systemGameManager);
                    creditController.AttributionHighlightButton.Configure(systemGameManager);
                }
                (uINavigationControllers[1] as UINavigationGrid).NumRows = Mathf.CeilToInt((float)(uINavigationControllers[1].ActiveNavigableButtonCount) / 2f);

            }
        }

        public void CloseMenu() {
            uIManager.creditsWindow.CloseWindow();
        }

    }
}