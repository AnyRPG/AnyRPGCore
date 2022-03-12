using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "NewAnimatedAbility",menuName = "AnyRPG/Abilities/AnimatedAbility")]
    public class AnimatedAbility : BaseAbility {


        [SerializeField]
        private AnimatedAbilityProperties animatedAbilityProperties = new AnimatedAbilityProperties();

        public override BaseAbilityProperties AbilityProperties { get => animatedAbilityProperties; }

        /*
        public override void Convert() {
            animatedAbilityProperties.GetAnimatedAbilityProperties(this);
        }
        */




    }

}