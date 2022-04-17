using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class PetEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private PetEffectProperties effectProperties = new PetEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}
