using AnyRPG;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public abstract class AbilityEffectConfig {

        // this is a base class for all ability effect configs

        public virtual AbilityEffectProperties AbilityEffectProperties { get => null; }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {

            AbilityEffectProperties.SetupScriptableObjects(systemGameManager, describable);
        }

        public virtual string Convert(Ability ability, string pathName) {
            return string.Empty;
        }

        public void CopyResourceProperties(Ability ability, DescribableResource describableResource, string effectType) {
            describableResource.resourceName = $"{ability.resourceName} {effectType}";
            describableResource.icon = ability.icon;
            describableResource.iconBackgroundImage = ability.iconBackgroundImage;
        }

    }
}