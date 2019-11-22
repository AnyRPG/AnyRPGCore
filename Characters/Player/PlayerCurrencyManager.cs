using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class PlayerCurrencyManager : MonoBehaviour {

        //public event System.Action OnCurrencyChange = delegate { };

        private BaseCharacter baseCharacter;

        private Dictionary<string, CurrencySaveData> currencyList = new Dictionary<string, CurrencySaveData>();

        public Dictionary<string, CurrencySaveData> MyCurrencyList { get => currencyList; }

        protected void Awake() {
            //Debug.Log(gameObject.name + ".PlayerFactionManager.Awake()");
            baseCharacter = GetComponent<BaseCharacter>();
        }

        protected void Start() {
        }

        public void AddCurrency(string currency, int currencyAmount) {
            AddCurrency(SystemCurrencyManager.MyInstance.GetResource(currency), currencyAmount);
        }

        public void AddCurrency(Currency currency, int currencyAmount) {
            //Debug.Log(gameObject.name + ".PlayerCurrencyManager.AddCurrency(" + currency.MyName + ", " + currencyAmount + ")");
            //bool foundReputation = false;
            CurrencySaveData newSaveData = new CurrencySaveData();
            string keyName = SystemResourceManager.prepareStringForMatch(currency.MyName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                newSaveData = new CurrencySaveData();
                newSaveData.MyName = MyCurrencyList[keyName].MyName;
                newSaveData.MyAmount = currencyAmount + MyCurrencyList[keyName].MyAmount;
                MyCurrencyList[keyName] = newSaveData;
                RedistributeCurrency(currency);
                SystemEventManager.MyInstance.NotifyOnCurrencyChange();
                return;
            }

            newSaveData = new CurrencySaveData();
            newSaveData.MyName = currency.MyName;
            newSaveData.MyAmount = currencyAmount;
            MyCurrencyList[keyName] = newSaveData;
            //OnCurrencyChange();
            RedistributeCurrency(currency);
            SystemEventManager.MyInstance.NotifyOnCurrencyChange();
        }

        public bool SpendCurrency(Currency currency, int currencyAmount) {
            //Debug.Log(gameObject.name + ".PlayerCurrencyManager.SpendCurrency(" + currency.MyName + ", " + currencyAmount + ")");
            //bool foundReputation = false;
            CurrencySaveData newSaveData = new CurrencySaveData();
            string keyName = SystemResourceManager.prepareStringForMatch(currency.MyName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                if (GetConvertedValue(currency, currencyAmount) < GetBaseCurrencyValue(SystemCurrencyManager.MyInstance.GetResource(keyName)) ) {
                    //if (currencyAmount < MyCurrencyList[keyName].MyAmount) {
                    newSaveData = new CurrencySaveData();
                    newSaveData.MyName = MyCurrencyList[keyName].MyName;
                    newSaveData.MyAmount = MyCurrencyList[keyName].MyAmount - currencyAmount;
                    MyCurrencyList[keyName] = newSaveData;
                    RedistributeCurrency(currency);
                    SystemEventManager.MyInstance.NotifyOnCurrencyChange();
                    return true;
                } else {
                    return false;
                }
            }
            return false;
        }

        public CurrencyGroup FindCurrencyGroup(Currency currency) {
            foreach (CurrencyGroup currencyGroup in SystemCurrencyGroupManager.MyInstance.MyResourceList.Values) {
                if (currencyGroup.HasCurrency(currency)) {
                    return currencyGroup;
                }
            }
            return null;
        }

        public int GetConvertedValue(Currency currency, int currencyAmount) {
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

        public int GetBaseCurrencyValue(Currency currency) {
            CurrencyGroup currencyGroup = FindCurrencyGroup(currency);
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
            CurrencyGroup currencyGroup = FindCurrencyGroup(currency);
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
                    if (MyCurrencyList.ContainsKey(SystemResourceManager.prepareStringForMatch(newCurrencyValue.Key.MyName))) {
                        MyCurrencyList.Remove(SystemResourceManager.prepareStringForMatch(newCurrencyValue.Key.MyName));
                    }
                    CurrencySaveData newSaveData = new CurrencySaveData();
                    newSaveData = new CurrencySaveData();
                    newSaveData.MyName = newCurrencyValue.Key.MyName;
                    newSaveData.MyAmount = newCurrencyValue.Value;
                    MyCurrencyList[SystemResourceManager.prepareStringForMatch(newCurrencyValue.Key.MyName)] = newSaveData;
                }
                SystemEventManager.MyInstance.NotifyOnCurrencyChange();
            }
        }

        public int GetCurrencyAmount(Currency currency) {
            //Debug.Log(gameObject.name + ".PlayerCurrencyManager.GetCurrency(" + currency.MyName + ")");
            //bool foundReputation = false;
            string keyName = SystemResourceManager.prepareStringForMatch(currency.MyName);
            if (MyCurrencyList.ContainsKey(keyName)) {
                return MyCurrencyList[keyName].MyAmount;
            }

            // default return
            return 0;
        }



    }

}