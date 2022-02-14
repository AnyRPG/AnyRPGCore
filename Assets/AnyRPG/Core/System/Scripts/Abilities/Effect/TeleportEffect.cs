using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New TeleportEffect", menuName = "AnyRPG/Abilities/Effects/TeleportEffect")]
    public class TeleportEffect : InstantEffect {

        [SerializeField]
        private TeleportEffectProperties teleportEffectProperties = new TeleportEffectProperties();

        public AbilityEffectProperties EffectProperties { get => teleportEffectProperties; }


    }

}
