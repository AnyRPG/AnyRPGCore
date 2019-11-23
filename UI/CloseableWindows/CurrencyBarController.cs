using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CurrencyBarController : MonoBehaviour {

        [SerializeField]
        private GameObject currencyAmountPrefab;

        [SerializeField]
        private GameObject currencyAmountParent;

        protected bool eventSubscriptionsInitialized = false;

        private List<CurrencyAmountController> currencyAmountControllers = new List<CurrencyAmountController>();

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

        public void UpdateCurrencyAmount(Currency currency, int currencyAmount) {

            Dictionary<Currency, int> currencyList = CurrencyConverter.RedistributeCurrency(currency, currencyAmount);

            // despawn old ones
            foreach (CurrencyAmountController currencyAmountController in currencyAmountControllers) {
                Destroy(currencyAmountController.gameObject);
            }
            currencyAmountControllers.Clear();

            // spawn new ones
            foreach (KeyValuePair<Currency, int> currencyPair in currencyList) {
                GameObject go = Instantiate(currencyAmountPrefab, currencyAmountParent.transform);
                go.transform.SetAsFirstSibling();
                CurrencyAmountController currencyAmountController = go.GetComponent<CurrencyAmountController>();
                currencyAmountControllers.Add(currencyAmountController);
                if (currencyAmountController.MyCurrencyIcon != null) {
                    currencyAmountController.MyCurrencyIcon.SetDescribable(currencyPair.Key);
                }
                if (currencyAmountController.MyAmountText != null) {
                    currencyAmountController.MyAmountText.text = currencyPair.Value.ToString();
                }
            }
        }

    }
}