using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class InstantiatedCurrencyItem : InstantiatedItem {

        private CurrencyItem currencyItem = null;

        private string gainCurrencyString = string.Empty;
        //private int gainCurrencyAmount = 0;
        private CurrencyNode currencyNode;

        private Sprite overrideIcon = null;

        // game manager references
        private CurrencyConverter currencyConverter = null;

        //public string GainCurrencyName { get => gainCurrencyName; }
        //public int GainCurrencyAmount { get => gainCurrencyAmount; }
        public CurrencyNode CurrencyNode { get => currencyNode; }

        public override Sprite Icon {
            get {
                return overrideIcon;
            }
        }

        public InstantiatedCurrencyItem(SystemGameManager systemGameManager, long instanceId, CurrencyItem currencyItem, ItemQuality itemQuality) : base(systemGameManager, instanceId, currencyItem, itemQuality) {
            this.currencyItem = currencyItem;
            //gainCurrencyName = currencyItem.GainCurrencyName;
            //gainCurrencyAmount = currencyItem.GainCurrencyAmount;
            currencyNode = currencyItem.CurrencyNode;
            RecalculateCurrencyString();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            currencyConverter = systemGameManager.CurrencyConverter;
        }

        public void OverrideCurrency(string newGainCurrencyName, int newGainCurrencyAmount) {
            //gainCurrencyName = newGainCurrencyName;
            //gainCurrencyAmount = newGainCurrencyAmount;
            Currency tmpCurrency = systemDataFactory.GetResource<Currency>(newGainCurrencyName);
            if (tmpCurrency != null) {
                OverrideCurrency(tmpCurrency, newGainCurrencyAmount);
            } else {
                Debug.LogError($"CurrencyItem.SetupScriptableObjects(): Could not find currency : {newGainCurrencyName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
            }
        }

        public void OverrideCurrency(Currency currency, int newGainCurrencyAmount) {
            currencyNode = new CurrencyNode();
            currencyNode.currency = currency;
            currencyNode.Amount = newGainCurrencyAmount;
            RecalculateCurrencyString();
        }

        public override bool Use(UnitController sourceUnitController) {
            //Debug.Log("CurrencyItem.Use()");
            bool returnValue = base.Use(sourceUnitController);
            if (returnValue == false) {
                return false;
            }
            if (currencyNode.currency != null) {
                sourceUnitController.CharacterCurrencyManager.AddCurrency(currencyNode.currency, currencyNode.Amount);
            }
            Remove();
            return true;
        }

        public override ItemInstanceSaveData GetItemSaveData() {
            //Debug.Log($"InstantiatedCurrencyItem.GetSlotSaveData()");

            ItemInstanceSaveData saveData = base.GetItemSaveData();
            saveData.GainCurrencyName = currencyNode.currency.ResourceName;
            saveData.GainCurrencyAmount = currencyNode.Amount;
            //Debug.Log($"InstantiatedCurrencyItem.GetSlotSaveData(): gainCurrencyName = {gainCurrencyName}, gainCurrencyAmount = {gainCurrencyAmount}");
            return saveData;
        }

        public override void LoadSaveData(ItemInstanceSaveData itemInstanceSaveData) {
            //Debug.Log($"InstantiatedCurrencyItem.LoadSaveData({inventorySlotSaveData.ItemName}) name: {inventorySlotSaveData.gainCurrencyName} amount: {inventorySlotSaveData.gainCurrencyAmount}");

            base.LoadSaveData(itemInstanceSaveData);
            OverrideCurrency(itemInstanceSaveData.GainCurrencyName, itemInstanceSaveData.GainCurrencyAmount);
        }

        
        public override string GetSummary() {
            return base.GetSummary();
        }
        

        private void RecalculateCurrencyString() {
            // if this comes from the system currency item, there will be no currency node
            if (currencyNode.currency == null) {
                gainCurrencyString = string.Empty;
                overrideIcon = base.Icon;
                return;
            }

            CurrencyGroup currencyGroup = currencyConverter.FindCurrencyGroup(currencyNode.currency);
            // this currency is not part of a currency group, so just return the normal string
            if (currencyGroup == null) {
                gainCurrencyString = $"{currencyNode.Amount} {currencyNode.currency.DisplayName}";
                overrideIcon = currencyNode.currency.Icon;
                return;
            }

            // attemp redistribution
            Currency baseCurrency = currencyGroup.BaseCurrency;

            // convert incoming currency to the base amount
            int baseCurrencyAmount = currencyConverter.GetBaseCurrencyAmount(currencyNode.currency, currencyNode.Amount);

            // create return dictionary
            //Dictionary<Currency, int> returnDictionary = new Dictionary<Currency, int>();

            // create a sorted list of the redistribution of this base currency amount into the higher currencies in the group
            SortedDictionary<int, Currency> sortList = new SortedDictionary<int, Currency>();
            foreach (CurrencyGroupRate currencyGroupRate in currencyGroup.CurrencyGroupRates) {
                sortList.Add(currencyGroupRate.BaseMultiple, currencyGroupRate.Currency);
            }
            gainCurrencyString = string.Empty;
            int highestConversionRate = 0;
            Currency highestCurrency = currencyGroup.BaseCurrency;
            foreach (KeyValuePair<int, Currency> currencyGroupRate in sortList.Reverse()) {
                int exchangedAmount = 0;
                if (currencyGroupRate.Key <= baseCurrencyAmount) {
                    // we can add this currency
                    exchangedAmount = (int)Mathf.Floor((float)baseCurrencyAmount / (float)currencyGroupRate.Key);
                    baseCurrencyAmount -= (exchangedAmount * currencyGroupRate.Key);
                }
                if (exchangedAmount > 0 && currencyGroupRate.Key > highestConversionRate) {
                    highestConversionRate = currencyGroupRate.Key;
                    highestCurrency = currencyGroupRate.Value;
                }
                //returnDictionary.Add(currencyGroupRate.Value, exchangedAmount);
                if (exchangedAmount > 0) {
                    if (gainCurrencyString != string.Empty) {
                        gainCurrencyString += ", ";
                    }
                    gainCurrencyString += $"{exchangedAmount} {currencyGroupRate.Value.DisplayName}";
                }
            }
            if (baseCurrencyAmount > 0) {
                if (gainCurrencyString != string.Empty) {
                    gainCurrencyString += ", ";
                }
                gainCurrencyString += $"{baseCurrencyAmount} {currencyGroup.BaseCurrency.DisplayName}";
            }
            //returnDictionary.Add(currencyGroup.BaseCurrency, baseCurrencyAmount);
            displayName = gainCurrencyString;
            overrideIcon = highestCurrency.Icon;
        }


        public override string GetDescription() {
            //Debug.Log($"{item.ResourceName}.InstantiatedCurrencyItem.GetDescription()");

            return base.GetDescription() + currencyItem.GetCurrencyItemDescription(gainCurrencyString);
        }

        /*
        public override string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            //Debug.Log(DisplayName + ".CurrencyItem.GetSummary();");
            string tmpCurrencyName = string.Empty;
            if (currencyNode.currency != null) {
                tmpCurrencyName = currencyNode.currency.DisplayName;
            }
            return base.GetDescription(usedItemQuality, usedItemLevel) + string.Format("\n<color=green>Use: Gain {0} {1}</color>", tmpCurrencyName, currencyNode.Amount);
        }
        */

    }

}