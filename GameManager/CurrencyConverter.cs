using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public static class CurrencyConverter {

        // converts the currency amount to its base currency and returns it
        public static int GetConvertedValue(Currency currency, int currencyAmount) {
            CurrencyGroup currencyGroup = FindCurrencyGroup(currency);
            if (currencyGroup != null) {
                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;
                // convert everything in the group to the base amount
                if (SystemResourceManager.MatchResource(currency.MyName, currencyGroup.MyBaseCurrency.MyName)) {
                    return currencyAmount;
                }
                // the currency needs conversion
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    if (SystemResourceManager.MatchResource(currencyGroupRate.MyCurrency.MyName, currency.MyName)) {
                        return currencyGroupRate.MyBaseMultiple * currencyAmount;
                    }
                }
                return currencyAmount;
            }
            return currencyAmount;
        }

        // returns the base currency for any given currency if it is part of a group, otherwise, just returns itself
        public static Currency GetBaseCurrency(Currency currency) {

            CurrencyGroup currencyGroup = FindCurrencyGroup(currency);

            if (currencyGroup != null) {
                return currencyGroup.MyBaseCurrency;
            }
            return currency;
        }

        // finds a currency group that the currency belongs to, or returns null if it does not belong to a group
        public static CurrencyGroup FindCurrencyGroup(Currency currency) {
            foreach (CurrencyGroup currencyGroup in SystemCurrencyGroupManager.MyInstance.MyResourceList.Values) {
                if (currencyGroup.HasCurrency(currency)) {
                    return currencyGroup;
                }
            }
            return null;
        }

        /*
        public int GetCurrencyAmountFromList(Currency currency, List<KeyValuePair<Currency, int>>) {
            //Debug.Log(gameObject.name + ".PlayerCurrencyManager.GetCurrency(" + currency.MyName + ")");
            //bool foundReputation = false;
            string keyName = SystemResourceManager.prepareStringForMatch(currency.MyName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                return MyCurrencyList[keyName].MyAmount;
            }

            // default return
            return 0;
        }
        */

            // returns a list of the redistribution of any currency into its group components
        public static Dictionary<Currency, int> RedistributeCurrency(Currency currency, int currencyAmount) {

            // create return dictionary
            Dictionary<Currency, int> returnDictionary = new Dictionary<Currency, int>();

            CurrencyGroup currencyGroup = FindCurrencyGroup(currency);
            if (currencyGroup == null || currencyGroup.MyBaseCurrency == currency) {
                returnDictionary.Add(currency, currencyAmount);
                // base currency was the same as input currency or input currency was not part of group, return it directly
                return returnDictionary;
            }

            // attemp redistribution
            Currency baseCurrency = currencyGroup.MyBaseCurrency;

            // convert incoming currency to the base amount
            int baseCurrencyAmount = GetConvertedValue(currency, currencyAmount);

            // create a sorted list of the redistribution of this base currency amount into the higher currencies in the group
            SortedDictionary<int, Currency> sortList = new SortedDictionary<int, Currency>();
            foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                sortList.Add(currencyGroupRate.MyBaseMultiple, currencyGroupRate.MyCurrency);
            }
            foreach (KeyValuePair<int, Currency> currencyGroupRate in sortList.Reverse()) {
                if (currencyGroupRate.Key < baseCurrencyAmount) {
                    // we can add this currency
                    int exchangedAmount = (int)Mathf.Floor((float)baseCurrencyAmount / (float)currencyGroupRate.Key);
                    returnDictionary.Add(currencyGroupRate.Value, exchangedAmount);
                    baseCurrencyAmount -= (exchangedAmount * currencyGroupRate.Key);
                }
            }
            returnDictionary.Add(currencyGroup.MyBaseCurrency, baseCurrencyAmount);

            return returnDictionary;
        }

    }
}

