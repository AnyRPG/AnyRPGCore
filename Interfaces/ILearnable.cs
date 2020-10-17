using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface ILearnable {

        string DisplayName { get; }
        int RequiredLevel { get; }
        Sprite MyIcon { get; }

        string GetSummary();
        string GetShortDescription();
    }

}