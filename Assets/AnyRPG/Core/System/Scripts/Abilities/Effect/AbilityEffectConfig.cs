using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public abstract class AbilityEffectConfig {

        // this is a base class for all ability effect configs

        public virtual AbilityEffectProperties AbilityEffectProperties { get => null; }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {

            AbilityEffectProperties.SetupScriptableObjects(systemGameManager, describable);
        }

    }
}