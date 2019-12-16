using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "NewCurrency", menuName = "AnyRPG/Currencies/Currency")]
    public class Currency : DescribableResource {

        public override string GetSummary() {
            return string.Format("{0}\nCurrent Amount: {1}", description, PlayerManager.MyInstance.MyCharacter.MyPlayerCurrencyManager.GetCurrencyAmount(this));
            //return string.Format("{0}", description);
        }

    }

    [System.Serializable]
    public struct CurrencyNode {

        public Currency currency;
        public int MyAmount;

    }


}