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

        // the smallest currency in this group
        [SerializeField]
        private Currency baseCurrency;

        [SerializeField]
        private List<CurrencyGroupRate> currencyGroupRates = new List<CurrencyGroupRate>();

        public Currency MyBaseCurrency { get => baseCurrency; set => baseCurrency = value; }
        public List<CurrencyGroupRate> MyCurrencyGroupRates { get => currencyGroupRates; set => currencyGroupRates = value; }

        public bool HasCurrency(Currency currency) {
            //Debug.Log("CurrencyGroup.HasCurrency(" + (currency == null ? "null" : currency.MyName) + ")");
            if (currency == null) {
                return false;
            }
            if (baseCurrency == null) {
                Debug.Log("CurrencyGroup.HasCurrency(" + (currency == null ? "null" : currency.MyName) + "): basecurrency is null");
            }
            if (SystemResourceManager.MatchResource(baseCurrency.MyName, currency.MyName)) {
                return true;
            }
            foreach (CurrencyGroupRate currencyGroupRate in currencyGroupRates) {
                if (SystemResourceManager.MatchResource(currencyGroupRate.MyCurrency.MyName, currency.MyName)) {
                    return true;
                }
            }
            return false;
        }
    }

    [System.Serializable]
    public class CurrencyGroupRate {

        [SerializeField]
        private Currency currency;

        // the number of base currency needed to make one of this currency
        [SerializeField]
        private int baseMultiple;

        public Currency MyCurrency { get => currency; set => currency = value; }
        public int MyBaseMultiple { get => baseMultiple; set => baseMultiple = value; }
    }

}