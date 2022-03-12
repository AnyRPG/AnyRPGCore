using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InstantEffectAbility : BaseAbility {

        [SerializeField]
        private InstantEffectAbilityProperties instantEffectAbilityProperties = new InstantEffectAbilityProperties();

        public override BaseAbilityProperties AbilityProperties { get => instantEffectAbilityProperties; }

        /*
        public override void Convert() {
            instantEffectAbilityProperties.GetBaseAbilityProperties(this);
        }
        */

    }

}