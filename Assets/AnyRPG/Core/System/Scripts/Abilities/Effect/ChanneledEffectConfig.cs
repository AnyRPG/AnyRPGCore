using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class ChanneledEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private ChanneledEffectProperties effectProperties = new ChanneledEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}
