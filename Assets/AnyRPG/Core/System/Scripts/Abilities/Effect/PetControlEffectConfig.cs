using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class PetControlEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private PetControlEffectProperties effectProperties = new PetControlEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

    }
}
