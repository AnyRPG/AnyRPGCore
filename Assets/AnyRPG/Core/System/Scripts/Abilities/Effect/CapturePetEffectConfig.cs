using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class CapturePetEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private CapturePetEffectProperties effectProperties = new CapturePetEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}