using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class KnockBackEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private KnockBackEffectProperties effectProperties = new KnockBackEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}
