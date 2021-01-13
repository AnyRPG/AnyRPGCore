using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IAbilityCaster {

        IAbilityManager AbilityManager { get; }
        Transform transform { get; }
        GameObject gameObject { get; }
    }

}