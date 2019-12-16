using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[System.Serializable]
public class FactionNode : IDescribable {

    public Faction faction;
    public string factionName;
    public int reputationAmount;

    public Sprite MyIcon { get => faction.MyIcon; }

    public string MyName { get => faction.MyName; }

    public string GetDescription() {
        return faction.GetDescription(); ;
    }

    public string GetSummary() {
        return faction.GetSummary();
    }
}

}