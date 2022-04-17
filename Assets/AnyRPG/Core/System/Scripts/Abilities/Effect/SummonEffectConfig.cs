using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SummonEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private SummonEffectProperties effectProperties = new SummonEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}
