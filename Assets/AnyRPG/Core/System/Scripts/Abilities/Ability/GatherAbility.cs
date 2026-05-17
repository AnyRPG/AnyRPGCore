using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Gather Ability",menuName = "AnyRPG/Abilities/GatherAbility")]
    public class GatherAbility : Ability {

        [SerializeField]
        private GatherAbilityProperties gatherAbilityProperties = new GatherAbilityProperties();

        public override AbilityProperties AbilityProperties { get => gatherAbilityProperties; }

        /*
        public override void Convert() {
            gatherAbilityProperties.GetBaseAbilityProperties(this);
        }
        */

    }

}