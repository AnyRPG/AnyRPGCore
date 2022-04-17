using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New TeleportEffect", menuName = "AnyRPG/Abilities/Effects/TeleportEffect")]
    public class TeleportEffect : AbilityEffect {

        [SerializeField]
        private TeleportEffectProperties teleportEffectProperties = new TeleportEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => teleportEffectProperties; }

    }

}
