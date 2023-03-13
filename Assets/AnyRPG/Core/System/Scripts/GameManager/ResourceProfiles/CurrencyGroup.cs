using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

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

        public Currency MyBaseCurrency { get => baseCurrency; set => baseCurrency = value; }
        public List<CurrencyGroupRate> MyCurrencyGroupRates { get => currencyGroupRates; set => currencyGroupRates = value; }

        public bool HasCurrency(Currency currency) {
            //Debug.Log("CurrencyGroup.HasCurrency(" + (currency == null ? "null" : currency.DisplayName) + ")");
            if (currency == null) {
                return false;
            }
            if (baseCurrency == null) {
                Debug.Log("CurrencyGroup.HasCurrency(" + (currency == null ? "null" : currency.ResourceName) + "): basecurrency is null");
            }
            if (SystemDataFactory.MatchResource(baseCurrency.ResourceName, currency.ResourceName)) {
                return true;
            }
            foreach (CurrencyGroupRate currencyGroupRate in currencyGroupRates) {
                if (SystemDataFactory.MatchResource(currencyGroupRate.Currency.ResourceName, currency.ResourceName)) {
                    return true;
                }
            }
            return false;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            baseCurrency = null;
            if (baseCurrencyName != null) {
                Currency tmpCurrency = systemDataFactory.GetResource<Currency>(baseCurrencyName);
                if (tmpCurrency != null) {
                    baseCurrency = tmpCurrency;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability : " + baseCurrencyName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
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