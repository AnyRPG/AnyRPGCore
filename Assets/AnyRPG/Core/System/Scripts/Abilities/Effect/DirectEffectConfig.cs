using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    // NOTE: DIRECTEFFECT WILL CAST TICK AND COMPLETE, BUT NEVER HIT.  HIT MUST BE CAST BY PROJECTILE, AOE, OR CHANNELED
    [System.Serializable]
    public class DirectEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private DirectEffectProperties effectProperties = new DirectEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}