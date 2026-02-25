using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class ChanneledEffectProperties : DirectEffectProperties {

        [Tooltip("the amount of time to delay damage after spawning the prefab")]
        public float effectDelay = 0f;

        // game manager references
        protected PlayerManagerClient playerManagerClient = null;

        /*
        public void GetChanneledEffectProperties(ChanneledEffect effect) {

            effectDelay = effect.effectDelay;

            GetDirectEffectProperties(effect);
        }
        */

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        public override Dictionary<PrefabProfile, List<GameObject>> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + "ChanneledEffect.Cast(" + source + ", " + (target == null ? "null" : target.name) + ")");
            if (target == null) {
                // maybe target died or despawned in the middle of cast?
                //Debug.Log(DisplayName + "ChanneledEffect.Cast(" + source + ", " + (target == null ? "null" : target.name) + "): TARGE IS NULL");

                return null;
            }
            if (abilityEffectContext == null) {
                abilityEffectContext = new AbilityEffectContext(source);
            }
            Dictionary<PrefabProfile, List<GameObject>> returnObjects = source.AbilityManager.SpawnChanneledEffectPrefabs(target, originalTarget, this, abilityEffectContext);
            
            return returnObjects;
        }

        public override bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            if (target == null) {
                // channeled effect always requires target because the prefab object must have a start and end point
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + DisplayName + ". Channneled abilities must always have a target");
                }
                return false;
            }
            return base.CanUseOn(target, sourceCharacter, abilityEffectContext, playerInitiated, performRangeCheck);
        }

    }
}
