using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Direct Ability", menuName = "AnyRPG/Abilities/DirectAbility")]
    public class DirectAbility : InstantEffectAbility {

        [SerializeField]
        private DirectAbilityProperties directAbilityProperties = new DirectAbilityProperties();

        public override BaseAbilityProperties AbilityProperties { get => directAbilityProperties; }

        /*
        public override void Convert() {
            directAbilityProperties.GetBaseAbilityProperties(this);
        }
        */


    }
}