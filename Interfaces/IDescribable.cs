using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IDescribable {
        Sprite MyIcon { get; }
        string DisplayName { get; }
        string GetDescription();
        string GetSummary();
    }

}