using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class LengthEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private LengthEffectProperties effectProperties = new LengthEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}