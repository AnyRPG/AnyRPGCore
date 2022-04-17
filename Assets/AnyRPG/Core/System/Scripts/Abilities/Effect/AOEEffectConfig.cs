using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class AOEEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private AOEEffectProperties effectProperties = new AOEEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}