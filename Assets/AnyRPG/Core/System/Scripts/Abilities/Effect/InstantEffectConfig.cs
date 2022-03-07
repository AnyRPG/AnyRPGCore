using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class InstantEffectConfig : AbilityEffectConfig {
        
        [SerializeField]
        private InstantEffectProperties effectProperties = new InstantEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}