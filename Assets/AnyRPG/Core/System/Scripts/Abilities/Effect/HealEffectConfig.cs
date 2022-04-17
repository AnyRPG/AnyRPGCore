using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class HealEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private HealEffectProperties effectProperties = new HealEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}