using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // NOTE: DIRECTEFFECT WILL CAST TICK AND COMPLETE, BUT NEVER HIT.  HIT MUST BE CAST BY PROJECTILE, AOE, OR CHANNELED
    [CreateAssetMenu(fileName = "New DirectEffect",menuName = "AnyRPG/Abilities/Effects/DirectEffect")]
    public class DirectEffect : FixedLengthEffect {

        [SerializeField]
        private DirectEffectProperties directEffectProperties = new DirectEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => directEffectProperties; }


    }

}