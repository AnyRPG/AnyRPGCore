using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ChanneledEffect",menuName = "AnyRPG/Abilities/Effects/ChanneledEffect")]
    public class ChanneledEffect : AbilityEffect {

        [SerializeField]
        private ChanneledEffectProperties channeledEffectProperties = new ChanneledEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => channeledEffectProperties; }



    }
}
