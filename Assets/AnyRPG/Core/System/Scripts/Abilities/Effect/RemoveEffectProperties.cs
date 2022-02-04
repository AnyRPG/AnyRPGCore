using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class RemoveEffectProperties : InstantEffectProperties {

        // 0 is unlimited
        [SerializeField]
        private int maxClearEffects = 0;

        // default will only clear harmful effects

        // effect types that this ability can clear
        [SerializeField]
        [ResourceSelector(resourceType = typeof(StatusEffectType))]
        private List<string> effectTypeNames = new List<string>();

        private List<StatusEffectType> effectTypes = new List<StatusEffectType>();

        public int MaxClearEffects { get => maxClearEffects; set => maxClearEffects = value; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, string displayName) {

            base.SetupScriptableObjects(systemGameManager, displayName);

            if (effectTypeNames != null) {
                foreach (string statusEffectType in effectTypeNames) {
                    StatusEffectType tmpStatusEffectType = systemDataFactory.GetResource<StatusEffectType>(statusEffectType);
                    if (tmpStatusEffectType != null) {
                        effectTypes.Add(tmpStatusEffectType);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find status effect type: " + statusEffectType + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }

    }
}