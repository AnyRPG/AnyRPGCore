using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Currency Group", menuName = "AnyRPG/Currencies/CurrencyGroup")]
    [System.Serializable]
    public class CurrencyGroup : DescribableResource {

        [Header("Currency Group")]

        [Tooltip("the smallest currency in this group")]
        [SerializeField]
        private string baseCurrencyName = string.Empty;

        [SerializeField]
        private Currency baseCurrency = null;

        [Header("Exchange Rates")]

        [SerializeField]
        private List<CurrencyGroupRate> currencyGroupRates = new List<CurrencyGroupRate>();

        public Currency BaseCurrency { get => baseCurrency; set => baseCurrency = value; }
        public List<CurrencyGroupRate> CurrencyGroupRates { get => currencyGroupRates; set => currencyGroupRates = value; }

        public bool HasCurrency(Currency currency) {
            //Debug.Log("CurrencyGroup.HasCurrency(" + (currency == null ? "null" : currency.DisplayName) + ")");
            if (currency == null) {
                return false;
            }
            if (baseCurrency == null) {
                Debug.LogWarning($"CurrencyGroup.HasCurrency({(currency == null ? "null" : currency.ResourceName)}): basecurrency is null");
            }
            if (SystemDataUtility.MatchResource(baseCurrency.ResourceName, currency.ResourceName)) {
                return true;
            }
            foreach (CurrencyGroupRate currencyGroupRate in currencyGroupRates) {
                if (SystemDataUtility.MatchResource(currencyGroupRate.Currency.ResourceName, currency.ResourceName)) {
                    return true;
                }
            }
            return false;
        }

        public List<Currency> GetCurrencyList() {
            List<Currency> currencyList = new List<Currency>();

            // create a sorted list and work down from the largest denomination, carrying the remainders
            SortedDictionary<int, Currency> sortList = new SortedDictionary<int, Currency>();
            foreach (CurrencyGroupRate currencyGroupRate in currencyGroupRates) {
                sortList.Add(currencyGroupRate.BaseMultiple, currencyGroupRate.Currency);
            }

            //foreach (KeyValuePair<int, Currency> currencyGroupRate in sortList) {
            foreach (KeyValuePair<int, Currency> currencyGroupRate in sortList.Reverse()) {
                //Debug.Log($"CurrencyGroup.GetCurrencyList() adding {currencyGroupRate.Value}");
                currencyList.Add(currencyGroupRate.Value);
            }
            currencyList.Add(baseCurrency);
            return currencyList;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            baseCurrency = null;
            if (baseCurrencyName != null) {
                Currency tmpCurrency = systemDataFactory.GetResource<Currency>(baseCurrencyName);
                if (tmpCurrency != null) {
                    baseCurrency = tmpCurrency;
                } else {
                    Debug.LogError($"CurrencyGroup.SetupScriptableObjects(): Could not find currency {baseCurrencyName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }

            if (currencyGroupRates != null) {
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroupRates) {
                    currencyGroupRate.SetupScriptableObjects(systemDataFactory);
                }
            }


        }

    }

    [System.Serializable]
    public class CurrencyGroupRate {

        [SerializeField]
        private string currencyName = string.Empty;

        [SerializeField]
        private Currency currency = null;

        // the number of base currency needed to make one of this currency
        [SerializeField]
        private int baseMultiple = 10;

        public Currency Currency { get => currency; set => currency = value; }
        public int BaseMultiple { get => baseMultiple; set => baseMultiple = value; }

        public void SetupScriptableObjects(SystemDataFactory systemDataFactory) {
            currency = null;
            if (currencyName != null) {
                Currency tmpCurrency = systemDataFactory.GetResource<Currency>(currencyName);
                if (tmpCurrency != null) {
                    currency = tmpCurrency;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability : " + currencyName + " while inititalizing a currency group rate.  CHECK INSPECTOR");
                }
            }

        }
    }

}