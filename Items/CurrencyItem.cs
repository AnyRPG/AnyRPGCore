using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrencyItem", menuName = "Inventory/Items/CurrencyItem", order = 1)]
public class CurrencyItem : Item, IUseable {

    [SerializeField]
    private CurrencySaveData currencyNode;

    public CurrencySaveData MyCurrencyNode { get => currencyNode; }

    public override void Use() {
        //Debug.Log("CurrencyItem.Use()");
        base.Use();
        if (currencyNode.MyName != string.Empty) {
            PlayerManager.MyInstance.MyCharacter.MyPlayerCurrencyManager.AddCurrency(currencyNode.MyName, currencyNode.MyAmount);
        }
        Remove();
    }

    public override string GetSummary() {
        return base.GetSummary() + string.Format("\n<color=green>Use: Gain {0} {1}</color>", currencyNode.MyName, currencyNode.MyAmount);
    }

}
