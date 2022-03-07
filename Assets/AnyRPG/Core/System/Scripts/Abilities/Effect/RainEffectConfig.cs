using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class RainEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private RainEffectProperties effectProperties = new RainEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}