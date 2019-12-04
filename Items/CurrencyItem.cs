using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "CurrencyItem", menuName = "AnyRPG/Inventory/Items/CurrencyItem", order = 1)]
    public class CurrencyItem : Item, IUseable {

        [SerializeField]
        private CurrencySaveData currencyNode;

        public CurrencySaveData MyCurrencyNode { get => currencyNode; }

        public override bool Use() {
            //Debug.Log("CurrencyItem.Use()");
            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }
            if (currencyNode.MyName != string.Empty) {
                PlayerManager.MyInstance.MyCharacter.MyPlayerCurrencyManager.AddCurrency(currencyNode.MyName, currencyNode.MyAmount);
            }
            Remove();
            return true;
        }

        public override string GetSummary() {
            return base.GetSummary() + string.Format("\n<color=green>Use: Gain {0} {1}</color>", currencyNode.MyName, currencyNode.MyAmount);
        }

    }

}