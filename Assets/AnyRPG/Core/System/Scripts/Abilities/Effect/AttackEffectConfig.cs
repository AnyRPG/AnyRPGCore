using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class AttackEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private AttackEffectProperties effectProperties = new AttackEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}
