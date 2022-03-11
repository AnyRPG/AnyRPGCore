using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "NewChanneledAbility", menuName = "AnyRPG/Abilities/ChanneledAbility")]
    public class ChanneledAbility : InstantEffectAbility {

        // this class may no longer be necessary as all the functionality was moved to baseability, but destroying it would mean reconfiguring all the scriptableobjects that use it.
        [SerializeField]
        private ChanneledAbilityProperties channeledAbilityProperties = new ChanneledAbilityProperties();

        public override BaseAbilityProperties AbilityProperties { get => channeledAbilityProperties; }

        public override void Convert() {
            channeledAbilityProperties.GetBaseAbilityProperties(this);
        }


    }

}