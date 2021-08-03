using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class CharacterCurrencyManager {

        private BaseCharacter baseCharacter;

        private Dictionary<string, CurrencyNode> currencyList = new Dictionary<string, CurrencyNode>();

        public Dictionary<string, CurrencyNode> MyCurrencyList { get => currencyList; }

        public CharacterCurrencyManager (BaseCharacter baseCharacter) {
            this.baseCharacter = baseCharacter;
        }

        public void AddCurrency(Currency currency, int currencyAmount) {
            //Debug.Log(gameObject.name + ".PlayerCurrencyManager.AddCurrency(" + currency.MyName + ", " + currencyAmount + ")");
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
            //Debug.Log(gameObject.name + ".PlayerCurrencyManager.SpendCurrency(" + currency.MyName + ", " + currencyAmount + ")");
            //bool foundReputation = false;
            CurrencyNode newCurrencyNode = new CurrencyNode();
            string keyName = SystemDataFactory.PrepareStringForMatch(currency.DisplayName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                if (CurrencyConverter.GetConvertedValue(currency, currencyAmount) <= GetBaseCurrencyValue(currency) ) {
                    //if (currencyAmount < MyCurrencyList[keyName].MyAmount) {
                    newCurrencyNode = new CurrencyNode();
                    newCurrencyNode.currency = MyCurrencyList[keyName].currency;
                    newCurrencyNode.MyAmount = MyCurrencyList[keyName].MyAmount - currencyAmount;
                    MyCurrencyList[keyName] = newCurrencyNode;
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
            CurrencyGroup currencyGroup = CurrencyConverter.FindCurrencyGroup(currency);
            if (currencyGroup != null) {
                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;
                // convert everything in the group to the base amount
                int baseCurrencyAmount = SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterCurrencyManager.GetCurrencyAmount(baseCurrency);
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    baseCurrencyAmount += SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterCurrencyManager.GetCurrencyAmount(currencyGroupRate.MyCurrency) * currencyGroupRate.MyBaseMultiple;
                }
                return baseCurrencyAmount;
            }
            return SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterCurrencyManager.GetCurrencyAmount(currency);
        }

        public void RedistributeCurrency(Currency currency) {
            CurrencyGroup currencyGroup = CurrencyConverter.FindCurrencyGroup(currency);
            if (currencyGroup != null) {
                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;
                // convert everything in the group to the base amount
                int baseCurrencyAmount = SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterCurrencyManager.GetCurrencyAmount(baseCurrency);
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    baseCurrencyAmount += SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterCurrencyManager.GetCurrencyAmount(currencyGroupRate.MyCurrency) * currencyGroupRate.MyBaseMultiple;
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
            CurrencyGroup currencyGroup = SystemGameManager.Instance.SystemConfigurationManager.DefaultCurrencyGroup;
            Dictionary<Currency, int> returnDictionary = new Dictionary<Currency, int>();
            if (currencyGroup != null) {
                //Debug.Log("PlayerCurrencyManager.GetRedistributedCurrency(): default currency group returned: " + currencyGroup.MyName);
                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;
                // convert everything in the group to the base amount
                int baseCurrencyAmount = SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterCurrencyManager.GetCurrencyAmount(baseCurrency);
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    baseCurrencyAmount += SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterCurrencyManager.GetCurrencyAmount(currencyGroupRate.MyCurrency) * currencyGroupRate.MyBaseMultiple;
                }
                returnDictionary.Add(baseCurrency, baseCurrencyAmount);
                return returnDictionary;
            }
            return null;
        }

    }

}