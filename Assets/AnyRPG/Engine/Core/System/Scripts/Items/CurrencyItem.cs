using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "CurrencyItem", menuName = "AnyRPG/Inventory/Items/CurrencyItem", order = 1)]
    public class CurrencyItem : Item, IUseable {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Currency))]
        private string gainCurrencyName = string.Empty;

        [SerializeField]
        private int gainCurrencyAmount = 0;

        //[SerializeField]
        private CurrencyNode currencyNode;

        public CurrencyNode MyCurrencyNode { get => currencyNode; }

        public override bool Use() {
            //Debug.Log("CurrencyItem.Use()");
            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }
            if (currencyNode.currency != null) {
                playerManager.MyCharacter.CharacterCurrencyManager.AddCurrency(currencyNode.currency, currencyNode.Amount);
            }
            Remove();
            return true;
        }

        public override string GetSummary() {
            //Debug.Log(MyName + ".CurrencyItem.GetSummary();");
            string tmpCurrencyName = string.Empty;
            if (currencyNode.currency != null) {
                tmpCurrencyName = currencyNode.currency.DisplayName;
            }
            return base.GetSummary() + string.Format("\n<color=green>Use: Gain {0} {1}</color>", tmpCurrencyName, currencyNode.Amount);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            if (gainCurrencyName != null && gainCurrencyName != string.Empty) {
                Currency tmpCurrency = systemDataFactory.GetResource<Currency>(gainCurrencyName);
                if (tmpCurrency != null) {
                    currencyNode.currency = tmpCurrency;
                    currencyNode.Amount = gainCurrencyAmount;
                } else {
                    Debug.LogError("CurrencyItem.SetupScriptableObjects(): Could not find currency : " + gainCurrencyName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}