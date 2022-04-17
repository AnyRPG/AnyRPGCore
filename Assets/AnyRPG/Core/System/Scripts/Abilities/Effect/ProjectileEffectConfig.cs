using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class ProjectileEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private ProjectileEffectProperties effectProperties = new ProjectileEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}