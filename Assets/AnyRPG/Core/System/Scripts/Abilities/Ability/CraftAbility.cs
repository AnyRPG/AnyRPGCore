using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Craft Ability",menuName = "AnyRPG/Abilities/CraftAbility")]
    public class CraftAbility : Ability {

        [SerializeField]
        private CraftAbilityProperties craftAbilityProperties = new CraftAbilityProperties();

        public override AbilityProperties AbilityProperties { get => craftAbilityProperties; }

        public CraftAbilityProperties CraftAbilityProperties { get => craftAbilityProperties; }

        /*
        public override void Convert() {
            craftAbilityProperties.GetBaseAbilityProperties(this);
        }
        */



    }

}