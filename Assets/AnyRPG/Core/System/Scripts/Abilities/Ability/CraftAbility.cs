using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Craft Ability",menuName = "AnyRPG/Abilities/Effects/CraftAbility")]
    public class CraftAbility : BaseAbility {

        [SerializeField]
        private CraftAbilityProperties craftAbilityProperties = new CraftAbilityProperties();

        public override BaseAbilityProperties AbilityProperties { get => craftAbilityProperties; }
        
        /*
        public override void Convert() {
            craftAbilityProperties.GetBaseAbilityProperties(this);
        }
        */

       

    }

}