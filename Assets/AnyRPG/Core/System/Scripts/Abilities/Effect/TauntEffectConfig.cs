using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class TauntEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private TauntEffectProperties effectProperties = new TauntEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}
