using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class PlayerCurrencyManager : MonoBehaviour {

        //public event System.Action OnCurrencyChange = delegate { };

        private BaseCharacter baseCharacter;

        private Dictionary<string, CurrencyNode> currencyList = new Dictionary<string, CurrencyNode>();

        public Dictionary<string, CurrencyNode> MyCurrencyList { get => currencyList; }

        protected void Awake() {
            //Debug.Log(gameObject.name + ".PlayerFactionManager.Awake()");
            baseCharacter = GetComponent<BaseCharacter>();
        }

        protected void Start() {
        }

        public void AddCurrency(Currency currency, int currencyAmount) {
            //Debug.Log(gameObject.name + ".PlayerCurrencyManager.AddCurrency(" + currency.MyName + ", " + currencyAmount + ")");
            //bool foundReputation = false;
            if (currency == null) {
                return;
            }
            CurrencyNode newCurrencyNode = new CurrencyNode();
            string keyName = SystemResourceManager.prepareStringForMatch(currency.MyDisplayName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                newCurrencyNode = new CurrencyNode();
                newCurrencyNode.currency = MyCurrencyList[keyName].currency;
                newCurrencyNode.MyAmount = currencyAmount + MyCurrencyList[keyName].MyAmount;
                MyCurrencyList[keyName] = newCurrencyNode;
                RedistributeCurrency(currency);
                SystemEventManager.MyInstance.NotifyOnCurrencyChange();
                return;
            }

            newCurrencyNode = new CurrencyNode();
            newCurrencyNode.currency = currency;
            newCurrencyNode.MyAmount = currencyAmount;
            MyCurrencyList[keyName] = newCurrencyNode;
            //OnCurrencyChange();
            RedistributeCurrency(currency);
            SystemEventManager.MyInstance.NotifyOnCurrencyChange();
        }

        public bool SpendCurrency(Currency currency, int currencyAmount) {
            //Debug.Log(gameObject.name + ".PlayerCurrencyManager.SpendCurrency(" + currency.MyName + ", " + currencyAmount + ")");
            //bool foundReputation = false;
            CurrencyNode newCurrencyNode = new CurrencyNode();
            string keyName = SystemResourceManager.prepareStringForMatch(currency.MyDisplayName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                if (CurrencyConverter.GetConvertedValue(currency, currencyAmount) <= GetBaseCurrencyValue(currency) ) {
                    //if (currencyAmount < MyCurrencyList[keyName].MyAmount) {
                    newCurrencyNode = new CurrencyNode();
                    newCurrencyNode.currency = MyCurrencyList[keyName].currency;
                    newCurrencyNode.MyAmount = MyCurrencyList[keyName].MyAmount - currencyAmount;
                    MyCurrencyList[keyName] = newCurrencyNode;
                    RedistributeCurrency(currency);
                    SystemEventManager.MyInstance.NotifyOnCurrencyChange();
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
                int baseCurrencyAmount = (PlayerManager.MyInstance.MyCharacter as PlayerCharacter).MyPlayerCurrencyManager.GetCurrencyAmount(baseCurrency);
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    baseCurrencyAmount += (PlayerManager.MyInstance.MyCharacter as PlayerCharacter).MyPlayerCurrencyManager.GetCurrencyAmount(currencyGroupRate.MyCurrency) * currencyGroupRate.MyBaseMultiple;
                }
                return baseCurrencyAmount;
            }
            return (PlayerManager.MyInstance.MyCharacter as PlayerCharacter).MyPlayerCurrencyManager.GetCurrencyAmount(currency);
        }

        public void RedistributeCurrency(Currency currency) {
            CurrencyGroup currencyGroup = CurrencyConverter.FindCurrencyGroup(currency);
            if (currencyGroup != null) {
                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;
                // convert everything in the group to the base amount
                int baseCurrencyAmount = (PlayerManager.MyInstance.MyCharacter as PlayerCharacter).MyPlayerCurrencyManager.GetCurrencyAmount(baseCurrency);
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    baseCurrencyAmount += (PlayerManager.MyInstance.MyCharacter as PlayerCharacter).MyPlayerCurrencyManager.GetCurrencyAmount(currencyGroupRate.MyCurrency) * currencyGroupRate.MyBaseMultiple;
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
                    if (MyCurrencyList.ContainsKey(SystemResourceManager.prepareStringForMatch(newCurrencyValue.Key.MyDisplayName))) {
                        MyCurrencyList.Remove(SystemResourceManager.prepareStringForMatch(newCurrencyValue.Key.MyDisplayName));
                    }
                    CurrencyNode newSaveData = new CurrencyNode();
                    newSaveData = new CurrencyNode();
                    newSaveData.currency = newCurrencyValue.Key;
                    newSaveData.MyAmount = newCurrencyValue.Value;
                    MyCurrencyList[SystemResourceManager.prepareStringForMatch(newCurrencyValue.Key.MyDisplayName)] = newSaveData;
                }
                SystemEventManager.MyInstance.NotifyOnCurrencyChange();
            }
        }

        public int GetCurrencyAmount(Currency currency) {
            //Debug.Log(gameObject.name + ".PlayerCurrencyManager.GetCurrency(" + currency.MyName + ")");
            //bool foundReputation = false;
            string keyName = SystemResourceManager.prepareStringForMatch(currency.MyDisplayName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                return MyCurrencyList[keyName].MyAmount;
            }

            // default return
            return 0;
        }

        public Dictionary<Currency, int> GetRedistributedCurrency() {
            //Debug.Log("PlayerCurrencyManager.GetRedistributedCurrency()");
            CurrencyGroup currencyGroup = SystemConfigurationManager.MyInstance.MyDefaultCurrencyGroup;
            Dictionary<Currency, int> returnDictionary = new Dictionary<Currency, int>();
            if (currencyGroup != null) {
                //Debug.Log("PlayerCurrencyManager.GetRedistributedCurrency(): default currency group returned: " + currencyGroup.MyName);
                // attemp redistribution
                Currency baseCurrency = currencyGroup.MyBaseCurrency;
                // convert everything in the group to the base amount
                int baseCurrencyAmount = (PlayerManager.MyInstance.MyCharacter as PlayerCharacter).MyPlayerCurrencyManager.GetCurrencyAmount(baseCurrency);
                foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.MyCurrencyGroupRates) {
                    baseCurrencyAmount += (PlayerManager.MyInstance.MyCharacter as PlayerCharacter).MyPlayerCurrencyManager.GetCurrencyAmount(currencyGroupRate.MyCurrency) * currencyGroupRate.MyBaseMultiple;
                }
                returnDictionary.Add(baseCurrency, baseCurrencyAmount);
                return returnDictionary;
            }
            return null;
        }

    }

}