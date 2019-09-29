using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCurrency", menuName = "Currencies/Currency")]
public class Currency : DescribableResource {

    public override string GetSummary() {
        return string.Format("{0}\nCurrent Amount: {1}", description, PlayerManager.MyInstance.MyCharacter.MyPlayerCurrencyManager.GetCurrencyAmount(this));
    }

}
