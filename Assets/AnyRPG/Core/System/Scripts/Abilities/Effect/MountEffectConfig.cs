using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class MountEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private MountEffectProperties effectProperties = new MountEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}
