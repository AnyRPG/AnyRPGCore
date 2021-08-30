using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class CharacterCurrencyManager : ConfiguredClass {

        private BaseCharacter baseCharacter;

        private Dictionary<string, CurrencyNode> currencyList = new Dictionary<string, CurrencyNode>();

        // game manager references
        private CurrencyConverter currencyConverter = null;

        public Dictionary<string, CurrencyNode> CurrencyList { get => currencyList; }

        public CharacterCurrencyManager (BaseCharacter baseCharacter, SystemGameManager systemGameManager) {
            this.baseCharacter = baseCharacter;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            currencyConverter = systemGameManager.CurrencyConverter;
        }

        public void AddCurrency(Currency currency, int currencyAmount) {
            //Debug.Log(baseCharacter.gameObject.name + ".PlayerCurrencyManager.AddCurrency(" + currency.DisplayName + ", " + currencyAmount + ")");
            if (currency == null) {
                return;
            }
            CurrencyNode newCurrencyNode = new CurrencyNode();
            string keyName = SystemDataFactory.PrepareStringForMatch(currency.DisplayName);
            if (CurrencyList.ContainsKey(keyName)) {
                newCurrencyNode = new CurrencyNode();
                newCurrencyNode.currency = CurrencyList[keyName].currency;
                newCurrencyNode.Amount = currencyAmount + CurrencyList[keyName].Amount;
                CurrencyList[keyName] = newCurrencyNode;
                RedistributeCurrency(currency);
                SystemEventManager.TriggerEvent("OnCurrencyChange", new EventParamProperties());
                return;
            }

            newCurrencyNode = new CurrencyNode();
            newCurrencyNode.currency = currency;
            newCurrencyNode.Amount = currencyAmount;
            CurrencyList[keyName] = newCurrencyNode;
            //OnCurrencyChange();
            RedistributeCurrency(currency);
            SystemEventManager.TriggerEvent("OnCurrencyChange", new EventParamProperties());
        }

        public bool SpendCurrency(Currency currency, int currencyAmount) {
            //Debug.Log(baseCharacter.gameObject.name + ".PlayerCurrencyManager.SpendCurrency(" + currency.DisplayName + ", " + currencyAmount + ")");
            //bool foundReputation = false;
            CurrencyNode newCurrencyNode = new CurrencyNode();
            string keyName = SystemDataFactory.PrepareStringForMatch(currency.DisplayName);
            if (CurrencyList.ContainsKey(keyName)) {
                if (currencyConverter.GetBaseCurrencyAmount(currency, currencyAmount) <= GetBaseCurrencyValue(currency) ) {
                    //if (currencyAmount < MyCurrencyList[keyName].MyAmount) {
                    newCurrencyNode = new CurrencyNode();
                    newCurrencyNode.currency = CurrencyList[keyName].currency;
                    newCurrencyNode.Amount = CurrencyList[keyName].Amount - currencyAmount;
                    CurrencyList[keyName] = newCurrencyNode;
                    RedistributeCurrency(currency);
                    SystemEventManager.TriggerEvent("OnCurrencyChange", new EventParamProperties());
                    return true;
                } else {
                    return false;
                }
            }
            return false;
        }

        public int GetBaseCurrencyValue(Currency currency) {
            //Debug.Log("CharacterCurrencyManager.GetBaseCurrencyValue(" + currency.DisplayName + ")");
            CurrencyGroup currencyGroup = currencyConverter.FindCurrencyGroup(currency);
            if (currencyGroup != null) {
                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;
                // convert everything in the group to the base amount
                int baseCurrencyAmount = GetCurrencyAmount(baseCurrency);
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    baseCurrencyAmount += GetCurrencyAmount(currencyGroupRate.Currency) * currencyGroupRate.BaseMultiple;
                }
                //Debug.Log("CharacterCurrencyManager.GetBaseCurrencyValue(" + currency.DisplayName + ") return: " + baseCurrencyAmount);
                return baseCurrencyAmount;
            }
            return GetCurrencyAmount(currency);
        }

        public void RedistributeCurrency(Currency currency) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCurrencyManager.RedistributeCurrency(" + currency.DisplayName + ")");
            CurrencyGroup currencyGroup = currencyConverter.FindCurrencyGroup(currency);
            if (currencyGroup != null) {

                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;

                // convert everything in the group to the base amount
                int baseCurrencyAmount = GetCurrencyAmount(baseCurrency);
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    baseCurrencyAmount += GetCurrencyAmount(currencyGroupRate.Currency) * currencyGroupRate.BaseMultiple;
                }

                // create a sorted list and work down from the largest denomination, carrying the remainders
                SortedDictionary<int, Currency> sortList = new SortedDictionary<int, Currency>();
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    sortList.Add(currencyGroupRate.BaseMultiple, currencyGroupRate.Currency);
                }
                Dictionary<Currency, int> newAmountList = new Dictionary<Currency, int>();
                foreach (KeyValuePair<int, Currency> currencyGroupRate in sortList.Reverse()) {
                    if (currencyGroupRate.Key <= baseCurrencyAmount) {
                        // there is at least one of this currency group
                        int exchangedAmount = (int)Mathf.Floor((float)baseCurrencyAmount / (float)currencyGroupRate.Key);
                        newAmountList.Add(currencyGroupRate.Value, exchangedAmount);
                        baseCurrencyAmount -= (exchangedAmount * currencyGroupRate.Key);
                    } else {
                        // since the entire original value has been redistributed, any groups that have no amount need to be zero or the money will be duplicated
                        newAmountList.Add(currencyGroupRate.Value, 0);
                    }
                }
                // whatever is left over is the base amount
                newAmountList.Add(currencyGroup.MyBaseCurrency, baseCurrencyAmount);

                // replace the original values in the currency list with the redistributed ones
                foreach (KeyValuePair<Currency, int> newCurrencyValue in newAmountList) {
                    if (CurrencyList.ContainsKey(SystemDataFactory.PrepareStringForMatch(newCurrencyValue.Key.DisplayName))) {
                        CurrencyList.Remove(SystemDataFactory.PrepareStringForMatch(newCurrencyValue.Key.DisplayName));
                    }
                    CurrencyNode newSaveData = new CurrencyNode();
                    newSaveData = new CurrencyNode();
                    newSaveData.currency = newCurrencyValue.Key;
                    newSaveData.Amount = newCurrencyValue.Value;
                    CurrencyList[SystemDataFactory.PrepareStringForMatch(newCurrencyValue.Key.DisplayName)] = newSaveData;
                }
                SystemEventManager.TriggerEvent("OnCurrencyChange", new EventParamProperties());
            }
        }

        public int GetCurrencyAmount(Currency currency) {
            //Debug.Log(baseCharacter.gameObject.name + ".PlayerCurrencyManager.GetCurrency(" + currency.DisplayName + ")");
            string keyName = SystemDataFactory.PrepareStringForMatch(currency.DisplayName);
            if (CurrencyList.ContainsKey(keyName)) {
                return CurrencyList[keyName].Amount;
            }

            // default return
            return 0;
        }

        public Dictionary<Currency, int> GetRedistributedCurrency() {
            //Debug.Log("PlayerCurrencyManager.GetRedistributedCurrency()");
            CurrencyGroup currencyGroup = systemConfigurationManager.DefaultCurrencyGroup;
            Dictionary<Currency, int> returnDictionary = new Dictionary<Currency, int>();
            if (currencyGroup != null) {
                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;
                // convert everything in the group to the base amount
                int baseCurrencyAmount = GetCurrencyAmount(baseCurrency);
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    baseCurrencyAmount += GetCurrencyAmount(currencyGroupRate.Currency) * currencyGroupRate.BaseMultiple;
                }
                returnDictionary.Add(baseCurrency, baseCurrencyAmount);
                return returnDictionary;
            }
            return null;
        }

    }

}