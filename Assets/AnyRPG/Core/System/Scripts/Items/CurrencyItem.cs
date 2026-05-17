using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "CurrencyItem", menuName = "AnyRPG/Inventory/Items/CurrencyItem", order = 1)]
    public class CurrencyItem : Item {

        [Header("Currency")]

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Currency))]
        private string gainCurrencyName = string.Empty;

        [SerializeField]
        private int gainCurrencyAmount = 0;

        //[SerializeField]
        private CurrencyNode currencyNode;

        public CurrencyNode CurrencyNode { get => currencyNode; }
        public string GainCurrencyName { get => gainCurrencyName; }
        public int GainCurrencyAmount { get => gainCurrencyAmount; }

        public override InstantiatedItem GetNewInstantiatedItem(SystemGameManager systemGameManager, long itemId, Item item, ItemQuality usedItemQuality) {
            if ((item is CurrencyItem) == false) {
                return null;
            }
            return new InstantiatedCurrencyItem(systemGameManager, itemId, item as CurrencyItem, usedItemQuality);
        }

        public override string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            //Debug.Log($"CurrencyItem.GetDescription({(usedItemQuality == null ? "null" : usedItemQuality.ResourceName)}, {usedItemLevel});");

            string tmpCurrencyName = string.Empty;
            if (currencyNode.currency != null) {
                tmpCurrencyName = $"{currencyNode.Amount} {currencyNode.currency.DisplayName}";
            }
            return base.GetDescription(usedItemQuality, usedItemLevel) + GetCurrencyItemDescription(tmpCurrencyName);
        }

        public string GetCurrencyItemDescription(string gainCurrencyString) {
            //Debug.Log($"CurrencyItem.GetCurrencyItemDescription({currencyDisplayName}, {currencyGainAmount})");

            return string.Format("\n<color=green>Use: Gain {0}</color>", gainCurrencyString);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            if (gainCurrencyName != null && gainCurrencyName != string.Empty) {
                Currency tmpCurrency = systemDataFactory.GetResource<Currency>(gainCurrencyName);
                if (tmpCurrency != null) {
                    currencyNode.currency = tmpCurrency;
                    currencyNode.Amount = gainCurrencyAmount;
                } else {
                    Debug.LogError($"CurrencyItem.SetupScriptableObjects(): Could not find currency : {gainCurrencyName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }
        }

    }

}