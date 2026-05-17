using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CurrencyBarController : ConfiguredMonoBehaviour {

        //[SerializeField]
        //private GameObject currencyAmountPrefab = null;

        //[SerializeField]
        //private GameObject currencyAmountParent = null;

        [SerializeField]
        private TextMeshProUGUI priceText = null;

        protected bool eventSubscriptionsInitialized = false;

        private bool hideZeroAmounts = false;

        // game manager references
        private CurrencyConverter currencyConverter = null;

        [SerializeField]
        protected List<CurrencyAmountController> currencyAmountControllers = new List<CurrencyAmountController>();

        public bool HideZeroAmounts { get => hideZeroAmounts; set => hideZeroAmounts = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            foreach (CurrencyAmountController currencyAmountController in currencyAmountControllers) {
                currencyAmountController.Configure(systemGameManager);
            }
        }

        public void DisableTooltip() {
            foreach (CurrencyAmountController currencyAmountController in currencyAmountControllers) {
                currencyAmountController.DisableTooltip();
            }
        }

        public void SetToolTipTransform(RectTransform toolTipTransform) {
            foreach (CurrencyAmountController currencyAmountController in currencyAmountControllers) {
                currencyAmountController.SetToolTipTransform(toolTipTransform);
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            currencyConverter = systemGameManager.CurrencyConverter;
        }

        public void ClearCurrencyAmounts() {
            foreach (CurrencyAmountController currencyAmountController in currencyAmountControllers) {
                currencyAmountController.gameObject.SetActive(false);
            }
            //currencyAmountControllers.Clear();
            if (priceText != null) {
                priceText.gameObject.SetActive(false);
            }
        }

        public void UpdateCurrencyAmount(Currency currency, int currencyAmount) {
            string priceString = string.Empty;
            UpdateCurrencyAmount(currency, currencyAmount, priceString);
        }

        public void UpdateCurrencyAmount(Currency currency, int currencyAmount, string priceString) {
            //Debug.Log($"{gameObject.name}.CurrencyBarController.UpdateCurrencyAmount(" + currency.DisplayName + ", " + currencyAmount + ", " + priceString + ")");

            Dictionary<Currency, int> currencyList = currencyConverter.RedistributeCurrency(currency, currencyAmount);

            ClearCurrencyAmounts();

            if (priceText != null) {
                if (priceString != string.Empty) {
                    priceText.gameObject.SetActive(true);
                    priceText.text = priceString;
                }
            }
            int counter = 0;
            bool nonZeroFound = false;
            foreach (KeyValuePair<Currency, int> currencyPair in currencyList) {
                //Debug.Log($"{gameObject.name}.CurrencyBarController.UpdateCurrencyAmount(" + currency.DisplayName + ", " + currencyAmount + "): currencyPair.Key: " + currencyPair.Key + "; currencyPair.Value: " + currencyPair.Value);
                if (currencyAmountControllers.Count > counter) {
                    CurrencyAmountController currencyAmountController = currencyAmountControllers[counter];
                    currencyAmountController.gameObject.SetActive(true);
                    if (currencyPair.Value > 0 && nonZeroFound == false) {
                        nonZeroFound = true;
                    }
                    if (currencyAmountController.CurrencyIcon != null) {
                        if (currencyPair.Value == 0 && (nonZeroFound == false && hideZeroAmounts == true)) {
                            currencyAmountController.CurrencyIcon.SetDescribable(null);
                        } else {
                            currencyAmountController.CurrencyIcon.SetDescribable(currencyPair.Key);
                        }
                    }
                    if (currencyAmountController.AmountText != null) {
                        if (currencyPair.Value == 0 && (nonZeroFound == false && hideZeroAmounts == true)) {
                            currencyAmountController.AmountText.text = string.Empty;
                        } else {
                            currencyAmountController.AmountText.text = currencyPair.Value.ToString();
                        }
                    }
                }
                counter += 1;
            }
        }

    }
}