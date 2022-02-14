using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ChanneledEffect",menuName = "AnyRPG/Abilities/Effects/ChanneledEffect")]
    public class ChanneledEffect : DirectEffect {

        /*
        // the amount of time to delay damage after spawning the prefab
        public float effectDelay = 0f;

        // game manager references
        protected PlayerManager playerManager = null;
        */
        [SerializeField]
        private ChanneledEffectProperties channeledEffectProperties = new ChanneledEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => channeledEffectProperties; }



    }
}
