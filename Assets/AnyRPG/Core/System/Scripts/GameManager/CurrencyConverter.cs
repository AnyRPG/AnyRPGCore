using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CurrencyConverter : ConfiguredMonoBehaviour {

        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        // converts the currency amount to its base currency and returns it
        public int GetBaseCurrencyAmount(Currency currency, int currencyAmount) {
            //Debug.Log("CurrencyConverter.GetBaseCurrencyAmount(" + currency.DisplayName + ", " + currencyAmount + ")");
            CurrencyGroup currencyGroup = FindCurrencyGroup(currency);
            if (currencyGroup != null) {
                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;
                // convert everything in the group to the base amount
                if (SystemDataFactory.MatchResource(currency.DisplayName, currencyGroup.MyBaseCurrency.DisplayName)) {
                    //Debug.Log("CurrencyConverter.GetBaseCurrencyAmount(" + currency.DisplayName + ", " + currencyAmount + ") return: " + currencyAmount);
                    return currencyAmount;
                }
                // the currency needs conversion
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    if (SystemDataFactory.MatchResource(currencyGroupRate.Currency.DisplayName, currency.DisplayName)) {
                        //Debug.Log("CurrencyConverter.GetBaseCurrencyAmount(" + currency.DisplayName + ", " + currencyAmount + ") return: " + (currencyGroupRate.MyBaseMultiple * currencyAmount));
                        return currencyGroupRate.BaseMultiple * currencyAmount;
                    }
                }
                //Debug.Log("CurrencyConverter.GetBaseCurrencyAmount(" + currency.DisplayName + ", " + currencyAmount + ") return: " + currencyAmount);
                return currencyAmount;
            }
            //Debug.Log("CurrencyConverter.GetBaseCurrencyAmount(" + currency.DisplayName + ", " + currencyAmount + ") return: " + currencyAmount);
            return currencyAmount;
        }

        // returns the base currency for any given currency if it is part of a group, otherwise, just returns itself
        public Currency GetBaseCurrency(Currency currency) {

            CurrencyGroup currencyGroup = FindCurrencyGroup(currency);

            if (currencyGroup != null) {
                return currencyGroup.MyBaseCurrency;
            }
            return currency;
        }

        public KeyValuePair<Sprite, string> RecalculateValues(List<CurrencyNode> usedCurrencyNodes, bool setIcon = true) {
            //Debug.Log("CurrencyConverter.RecalculateValues()");
            Sprite returnSprite = null;
            List<string> returnStrings = new List<string>();
            Dictionary<Currency, CurrencyNode> squishedNodes = new Dictionary<Currency, CurrencyNode>();
            foreach (CurrencyNode currencyNode in usedCurrencyNodes) {
                if (currencyNode.currency != null) {
                    if (squishedNodes.ContainsKey(currencyNode.currency)) {
                        CurrencyNode tmp = squishedNodes[currencyNode.currency];
                        tmp.Amount += currencyNode.Amount;
                        squishedNodes[currencyNode.currency] = tmp;
                    } else {
                        CurrencyNode tmp = new CurrencyNode();
                        tmp.currency = currencyNode.currency;
                        tmp.Amount = currencyNode.Amount;
                        squishedNodes.Add(tmp.currency, tmp);
                    }
                }
            }
            if (squishedNodes.Count > 0) {
                //Debug.Log("LootableDrop.RecalculateValues(): squishedNodes.count: " + squishedNodes.Count);
                bool nonZeroFound = false;
                foreach (KeyValuePair<Currency, int> keyValuePair in RedistributeCurrency(squishedNodes.ElementAt(0).Value.currency, squishedNodes.ElementAt(0).Value.Amount)) {
                    if (keyValuePair.Value > 0 && nonZeroFound == false) {
                        nonZeroFound = true;
                        if (setIcon) {
                            returnSprite = keyValuePair.Key.Icon;
                        }
                    }
                    if (nonZeroFound == true) {
                        returnStrings.Add(keyValuePair.Value + " " + keyValuePair.Key.DisplayName);
                    }
                }
            }

            //Debug.Log("LootableDrop.RecalculateValues(): " + summary);
            return new KeyValuePair<Sprite, string>(returnSprite, string.Join("\n", returnStrings));
        }

        // finds a currency group that the currency belongs to, or returns null if it does not belong to a group
        public CurrencyGroup FindCurrencyGroup(Currency currency) {
            //Debug.Log("CurrencyConverter.FindCurrencyGroup(" + (currency == null ? "null" : currency.DisplayName) + ")");
            if (currency != null) {
                foreach (CurrencyGroup currencyGroup in systemDataFactory.GetResourceList<CurrencyGroup>()) {
                    if (currencyGroup.HasCurrency(currency)) {
                        return currencyGroup;
                    }
                }
            }
            return null;
        }

        /*
        public int GetCurrencyAmountFromList(Currency currency, List<KeyValuePair<Currency, int>>) {
            //Debug.Log(gameObject.name + ".PlayerCurrencyManager.GetCurrency(" + currency.DisplayName + ")");
            //bool foundReputation = false;
            string keyName = SystemDataFactory.PrepareStringForMatch(currency.DisplayName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                return MyCurrencyList[keyName].MyAmount;
            }

            // default return
            return 0;
        }
        */

            // returns a list of the redistribution of any currency into its group components
        public Dictionary<Currency, int> RedistributeCurrency(Currency currency, int currencyAmount) {

            // create return dictionary
            Dictionary<Currency, int> returnDictionary = new Dictionary<Currency, int>();

            CurrencyGroup currencyGroup = FindCurrencyGroup(currency);
            if (currencyGroup == null) {
                returnDictionary.Add(currency, currencyAmount);
                // base currency was the same as input currency or input currency was not part of group, return it directly
                return returnDictionary;
            }

            // attemp redistribution
            Currency baseCurrency = currencyGroup.MyBaseCurrency;

            // convert incoming currency to the base amount
            int baseCurrencyAmount = GetBaseCurrencyAmount(currency, currencyAmount);

            // create a sorted list of the redistribution of this base currency amount into the higher currencies in the group
            SortedDictionary<int, Currency> sortList = new SortedDictionary<int, Currency>();
            foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                sortList.Add(currencyGroupRate.BaseMultiple, currencyGroupRate.Currency);
            }
            foreach (KeyValuePair<int, Currency> currencyGroupRate in sortList.Reverse()) {
                int exchangedAmount = 0;
                if (currencyGroupRate.Key <= baseCurrencyAmount) {
                    // we can add this currency
                    exchangedAmount = (int)Mathf.Floor((float)baseCurrencyAmount / (float)currencyGroupRate.Key);
                    baseCurrencyAmount -= (exchangedAmount * currencyGroupRate.Key);
                }
                returnDictionary.Add(currencyGroupRate.Value, exchangedAmount);
            }
            returnDictionary.Add(currencyGroup.MyBaseCurrency, baseCurrencyAmount);

            return returnDictionary;
        }

        public string GetCombinedPriceString(KeyValuePair<Currency, int> keyValuePair) {
            return GetCombinedPriceString(keyValuePair.Key, keyValuePair.Value);
        }

        public string GetCombinedPriceString(Currency currency, int currencyAmount) {
            string returnValue = string.Empty;
            Dictionary<Currency, int> tmpDict = RedistributeCurrency(currency, currencyAmount);
            foreach (KeyValuePair<Currency, int> dictEntry in tmpDict) {
                returnValue += dictEntry.Value.ToString() + " " + dictEntry.Key.DisplayName + " ";
            }
            return returnValue;
        }

    }
}

