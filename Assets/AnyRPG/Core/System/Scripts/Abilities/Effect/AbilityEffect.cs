using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New AbilityEffect", menuName = "AnyRPG/Abilities/Effects/AbilityEffect")]
    public class AbilityEffect : DescribableResource {

        public virtual AbilityEffectProperties AbilityEffectProperties { get => null; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            
            //Debug.Log(resourceName);

            AbilityEffectProperties.SetupScriptableObjects(systemGameManager, this);
        }
    }
}