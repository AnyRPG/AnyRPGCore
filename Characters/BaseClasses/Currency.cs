using AnyRPG;
﻿using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace AnyRPG {
[CreateAssetMenu(fileName = "NewCurrency",menuName = "AnyRPG/Currencies/Currency")]
public class Currency : DescribableResource {

    public override string GetSummary() {
        return string.Format("{0}\nCurrent Amount: {1}", description, PlayerManager.MyInstance.MyCharacter.MyPlayerCurrencyManager.GetCurrencyAmount(this));
    }

}

}