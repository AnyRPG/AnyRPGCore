using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class TeleportEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private TeleportEffectProperties effectProperties = new TeleportEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}
