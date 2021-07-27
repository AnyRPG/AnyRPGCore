using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New RemoveEffect", menuName = "AnyRPG/Abilities/Effects/RemoveEffect")]
    public class RemoveEffect : InstantEffect {

        // 0 is unlimited
        [SerializeField]
        private int maxClearEffects = 0;

        // default will only clear harmful effects

        // effect types that this ability can clear
        [SerializeField]
        private List<string> effectTypeNames = new List<string>();

        private List<StatusEffectType> effectTypes = new List<StatusEffectType>();

        public override void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            base.PerformAbilityHit(source, target, abilityEffectInput);

            List<StatusEffectNode> removeEffects = new List<StatusEffectNode>();

            CharacterUnit targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
            if (targetCharacterUnit != null && targetCharacterUnit.BaseCharacter != null && targetCharacterUnit.BaseCharacter.CharacterStats != null) {
                foreach (StatusEffectNode statusEffectNode in targetCharacterUnit.BaseCharacter.CharacterStats.StatusEffects.Values) {
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


        public override void SetupScriptableObjects() {

            base.SetupScriptableObjects();

            if (effectTypeNames != null) {
                foreach (string statusEffectType in effectTypeNames) {
                    StatusEffectType tmpStatusEffectType = SystemStatusEffectTypeManager.Instance.GetResource(statusEffectType);
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