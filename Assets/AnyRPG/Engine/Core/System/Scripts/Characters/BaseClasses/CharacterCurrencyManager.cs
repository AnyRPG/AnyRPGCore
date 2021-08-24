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

        public Dictionary<string, CurrencyNode> MyCurrencyList { get => currencyList; }

        public CharacterCurrencyManager (BaseCharacter baseCharacter, SystemGameManager systemGameManager) {
            this.baseCharacter = baseCharacter;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            currencyConverter = systemGameManager.CurrencyConverter;
        }

        public void AddCurrency(Currency currency, int currencyAmount) {
            Debug.Log(baseCharacter.gameObject.name + ".PlayerCurrencyManager.AddCurrency(" + currency.DisplayName + ", " + currencyAmount + ")");
            //bool foundReputation = false;
            if (currency == null) {
                return;
            }
            CurrencyNode newCurrencyNode = new CurrencyNode();
            string keyName = SystemDataFactory.PrepareStringForMatch(currency.DisplayName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                newCurrencyNode = new CurrencyNode();
                newCurrencyNode.currency = MyCurrencyList[keyName].currency;
                newCurrencyNode.MyAmount = currencyAmount + MyCurrencyList[keyName].MyAmount;
                MyCurrencyList[keyName] = newCurrencyNode;
                RedistributeCurrency(currency);
                SystemEventManager.TriggerEvent("OnCurrencyChange", new EventParamProperties());
                return;
            }

            newCurrencyNode = new CurrencyNode();
            newCurrencyNode.currency = currency;
            newCurrencyNode.MyAmount = currencyAmount;
            MyCurrencyList[keyName] = newCurrencyNode;
            //OnCurrencyChange();
            RedistributeCurrency(currency);
            SystemEventManager.TriggerEvent("OnCurrencyChange", new EventParamProperties());
        }

        public bool SpendCurrency(Currency currency, int currencyAmount) {
            Debug.Log(baseCharacter.gameObject.name + ".PlayerCurrencyManager.SpendCurrency(" + currency.DisplayName + ", " + currencyAmount + ")");
            //bool foundReputation = false;
            CurrencyNode newCurrencyNode = new CurrencyNode();
            string keyName = SystemDataFactory.PrepareStringForMatch(currency.DisplayName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                if (currencyConverter.GetBaseCurrencyAmount(currency, currencyAmount) <= GetBaseCurrencyValue(currency) ) {
                    //if (currencyAmount < MyCurrencyList[keyName].MyAmount) {
                    newCurrencyNode = new CurrencyNode();
                    newCurrencyNode.currency = MyCurrencyList[keyName].currency;
                    newCurrencyNode.MyAmount = MyCurrencyList[keyName].MyAmount - currencyAmount;
                    MyCurrencyList[keyName] = newCurrencyNode;
                    RedistributeCurrency(currency);
                    SystemEventManager.TriggerEvent("OnCurrencyChange", new EventParamProperties());
                    return true;
                } else {
                    Debug.Log(baseCharacter.gameObject.name + ".PlayerCurrencyManager.SpendCurrency(" + currency.DisplayName + ", " + currencyAmount + ") return false");
                    return false;
                }
            }
            Debug.Log(baseCharacter.gameObject.name + ".PlayerCurrencyManager.SpendCurrency(" + currency.DisplayName + ", " + currencyAmount + ") return false");
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
                    baseCurrencyAmount += GetCurrencyAmount(currencyGroupRate.MyCurrency) * currencyGroupRate.MyBaseMultiple;
                }
                //Debug.Log("CharacterCurrencyManager.GetBaseCurrencyValue(" + currency.DisplayName + ") return: " + baseCurrencyAmount);
                return baseCurrencyAmount;
            }
            return GetCurrencyAmount(currency);
        }

        public void RedistributeCurrency(Currency currency) {
            Debug.Log(baseCharacter.gameObject.name + ".CharacterCurrencyManager.GetBaseCurrencyValue(" + currency.DisplayName + ")");
            CurrencyGroup currencyGroup = currencyConverter.FindCurrencyGroup(currency);
            if (currencyGroup != null) {
                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;
                // convert everything in the group to the base amount
                int baseCurrencyAmount = GetCurrencyAmount(baseCurrency);
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    baseCurrencyAmount += GetCurrencyAmount(currencyGroupRate.MyCurrency) * currencyGroupRate.MyBaseMultiple;
                }
                SortedDictionary<int, Currency> sortList = new SortedDictionary<int, Currency>();
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    sortList.Add(currencyGroupRate.MyBaseMultiple, currencyGroupRate.MyCurrency);
                }
                Dictionary<Currency, int> newAmountList = new Dictionary<Currency, int>();
                foreach (KeyValuePair<int, Currency> currencyGroupRate in sortList.Reverse()) {
                    if (currencyGroupRate.Key < baseCurrencyAmount) {
                        // we can add this currency
                        int exchangedAmount = (int)Mathf.Floor((float)baseCurrencyAmount / (float)currencyGroupRate.Key);
                        newAmountList.Add(currencyGroupRate.Value, exchangedAmount);
                        baseCurrencyAmount -= (exchangedAmount * currencyGroupRate.Key);
                    }
                }
                newAmountList.Add(currencyGroup.MyBaseCurrency, baseCurrencyAmount);
                foreach (KeyValuePair<Currency, int> newCurrencyValue in newAmountList) {
                    if (MyCurrencyList.ContainsKey(SystemDataFactory.PrepareStringForMatch(newCurrencyValue.Key.DisplayName))) {
                        MyCurrencyList.Remove(SystemDataFactory.PrepareStringForMatch(newCurrencyValue.Key.DisplayName));
                    }
                    CurrencyNode newSaveData = new CurrencyNode();
                    newSaveData = new CurrencyNode();
                    newSaveData.currency = newCurrencyValue.Key;
                    newSaveData.MyAmount = newCurrencyValue.Value;
                    MyCurrencyList[SystemDataFactory.PrepareStringForMatch(newCurrencyValue.Key.DisplayName)] = newSaveData;
                }
                SystemEventManager.TriggerEvent("OnCurrencyChange", new EventParamProperties());
            }
        }

        public int GetCurrencyAmount(Currency currency) {
            //Debug.Log(gameObject.name + ".PlayerCurrencyManager.GetCurrency(" + currency.MyName + ")");
            //bool foundReputation = false;
            string keyName = SystemDataFactory.PrepareStringForMatch(currency.DisplayName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                return MyCurrencyList[keyName].MyAmount;
            }

            // default return
            return 0;
        }

        public Dictionary<Currency, int> GetRedistributedCurrency() {
            //Debug.Log("PlayerCurrencyManager.GetRedistributedCurrency()");
            CurrencyGroup currencyGroup = systemConfigurationManager.DefaultCurrencyGroup;
            Dictionary<Currency, int> returnDictionary = new Dictionary<Currency, int>();
            if (currencyGroup != null) {
                //Debug.Log("PlayerCurrencyManager.GetRedistributedCurrency(): default currency group returned: " + currencyGroup.MyName);
                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;
                // convert everything in the group to the base amount
                int baseCurrencyAmount = GetCurrencyAmount(baseCurrency);
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    baseCurrencyAmount += GetCurrencyAmount(currencyGroupRate.MyCurrency) * currencyGroupRate.MyBaseMultiple;
                }
                returnDictionary.Add(baseCurrency, baseCurrencyAmount);
                return returnDictionary;
            }
            return null;
        }

    }

}