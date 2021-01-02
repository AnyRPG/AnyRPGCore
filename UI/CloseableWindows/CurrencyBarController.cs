using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CurrencyBarController : MonoBehaviour {

        //[SerializeField]
        //private GameObject currencyAmountPrefab = null;

        //[SerializeField]
        //private GameObject currencyAmountParent = null;

        [SerializeField]
        private TextMeshProUGUI priceText = null;

        protected bool eventSubscriptionsInitialized = false;

        [SerializeField]
        protected List<CurrencyAmountController> currencyAmountControllers = new List<CurrencyAmountController>();

        public void Awake() {
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("VendorUI.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("UnitSpawnNode.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = false;
        }

        public void OnDestroy() {
            //Debug.Log("UnitSpawnNode.OnDisable(): stopping any outstanding coroutines");
            CleanupEventSubscriptions();
        }

        public void ClearCurrencyAmounts() {
            foreach (CurrencyAmountController currencyAmountController in currencyAmountControllers) {
                currencyAmountController.gameObject.SetActive(false);
            }
            //currencyAmountControllers.Clear();
            priceText.gameObject.SetActive(false);
        }

        public void UpdateCurrencyAmount(Currency currency, int currencyAmount) {
            string priceString = string.Empty;
            UpdateCurrencyAmount(currency, currencyAmount, priceString);
        }

        public void UpdateCurrencyAmount(Currency currency, int currencyAmount, string priceString) {
            //Debug.Log(gameObject.name + ".CurrencyBarController.UpdateCurrencyAmount(" + currency.DisplayName + ", " + currencyAmount + ", " + priceString + ")");

            Dictionary<Currency, int> currencyList = CurrencyConverter.RedistributeCurrency(currency, currencyAmount);

            ClearCurrencyAmounts();
            // spawn new ones

            if (priceText != null) {
                if (priceString != string.Empty) {
                    priceText.gameObject.SetActive(true);
                    priceText.text = priceString;
                }
            }
            int counter = 0;
            foreach (KeyValuePair<Currency, int> currencyPair in currencyList) {
                //Debug.Log(gameObject.name + ".CurrencyBarController.UpdateCurrencyAmount(" + currency.MyName + ", " + currencyAmount + "): currencyPair.Key: " + currencyPair.Key + "; currencyPair.Value: " + currencyPair.Value);
                //GameObject go = Instantiate(currencyAmountPrefab, currencyAmountParent.transform);
                //go.transform.SetAsFirstSibling();
                //CurrencyAmountController currencyAmountController = go.GetComponent<CurrencyAmountController>();
                //currencyAmountControllers.Add(currencyAmountController);
                if (currencyAmountControllers.Count > counter) {
                    CurrencyAmountController currencyAmountController = currencyAmountControllers[counter];
                    currencyAmountController.gameObject.SetActive(true);
                    if (currencyAmountController.MyCurrencyIcon != null) {
                        currencyAmountController.MyCurrencyIcon.SetDescribable(currencyPair.Key);
                    }
                    if (currencyAmountController.MyAmountText != null) {
                        currencyAmountController.MyAmountText.text = currencyPair.Value.ToString();
                    }
                }
                counter += 1;
            }
        }

    }
}