using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public abstract class StatusEffectConfig : AbilityEffectConfig {
        // abstract for now because we can't have inline status effects due to the need to do a database lookup to re-apply saved effects on game load
        // making it abstract will prevent it from showing up on dropdown lists

        [SerializeField]
        private StatusEffectProperties effectProperties = new StatusEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}
