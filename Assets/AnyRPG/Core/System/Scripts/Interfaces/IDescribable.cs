using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IDescribable {
        Sprite Icon { get; }
        string DisplayName { get; }
        string Description { get; }
        string GetSummary();
        string GetDescription();
    }

}