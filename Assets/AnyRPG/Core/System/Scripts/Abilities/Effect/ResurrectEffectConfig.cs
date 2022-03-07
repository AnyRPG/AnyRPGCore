using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class ResurrectEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private ResurrectEffectProperties effectProperties = new ResurrectEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}