using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class StatusEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private StatusEffectProperties effectProperties = new StatusEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}
