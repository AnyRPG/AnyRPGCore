using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class RemoveEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private RemoveEffectProperties effectProperties = new RemoveEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}