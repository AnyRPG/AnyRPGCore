using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class RemoveEffectProperties : InstantEffectProperties {

        [Header("Remove Effect")]

        [Tooltip("The maximum amount of effects to remove.  0 is unlimited.")]
        [SerializeField]
        private int maxClearEffects = 0;

        // default will only clear harmful effects

        [Tooltip("Effect types that this ability can clear")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(StatusEffectType))]
        private List<string> effectTypeNames = new List<string>();

        private List<StatusEffectType> effectTypes = new List<StatusEffectType>();

        /*
        public void GetRemoveEffectProperties(RemoveEffect effect) {

            maxClearEffects = effect.MaxClearEffects;
            effectTypeNames = effect.EffectTypeNames;

            GetInstantEffectProperties(effect);
        }
        */

        public override void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            base.PerformAbilityHit(source, target, abilityEffectInput);

            List<StatusEffectNode> removeEffects = new List<StatusEffectNode>();

            CharacterUnit targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
            if (targetCharacterUnit != null) {
                foreach (StatusEffectNode statusEffectNode in targetCharacterUnit.UnitController.CharacterStats.StatusEffects.Values) {
                    if (statusEffectNode.StatusEffect.StatusEffectType != null && effectTypes.Contains(statusEffectNode.StatusEffect.StatusEffectType)) {
                        removeEffects.Add(statusEffectNode);
                    }
                    if (maxClearEffects != 0 && removeEffects.Count >= maxClearEffects) {
                        break;
                    }
                }

                foreach (StatusEffectNode statusEffectNode in removeEffects) {
                    statusEffectNode.CancelStatusEffect();
                }
            }
        }


        public override void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {

            base.SetupScriptableObjects(systemGameManager, describable);

            if (effectTypeNames != null) {
                foreach (string statusEffectType in effectTypeNames) {
                    StatusEffectType tmpStatusEffectType = systemDataFactory.GetResource<StatusEffectType>(statusEffectType);
                    if (tmpStatusEffectType != null) {
                        effectTypes.Add(tmpStatusEffectType);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find status effect type: " + statusEffectType + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }

    }
}