using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IAbilityProvider {

        List<StatusEffect> TraitList { get; set; }
        List<BaseAbility> AbilityList { get; set; }

    }

}