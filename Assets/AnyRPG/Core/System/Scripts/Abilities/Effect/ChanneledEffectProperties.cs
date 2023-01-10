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
        protected PlayerManager playerManager = null;

        /*
        public void GetChanneledEffectProperties(ChanneledEffect effect) {

            effectDelay = effect.effectDelay;

            GetDirectEffectProperties(effect);
        }
        */

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
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
            Dictionary<PrefabProfile, List<GameObject>> returnObjects = base.Cast(source, target, originalTarget, abilityEffectContext);
            if (returnObjects != null) {
                //Debug.Log(DisplayName + "ChanneledEffect.Cast(" + source + ", " + (target == null ? "null" : target.name) + ") PREFABOBJECTS WAS NOT NULL");

                foreach (PrefabProfile prefabProfile in returnObjects.Keys) {
                    foreach (GameObject go in returnObjects[prefabProfile]) {
                        // recently added code will properly spawn the object based on universal attachments
                        // get references to the parent and rotation to pass them onto the channeled object script
                        // since this object will switch parents to avoid moving/rotating with the character body
                        GameObject prefabParent = go.transform.parent.gameObject;
                        Vector3 sourcePosition = go.transform.localPosition;

                        go.transform.parent = playerManager.EffectPrefabParent.transform;
                        IChanneledObject channeledObjectScript = go.GetComponent<IChanneledObject>();
                        if (channeledObjectScript != null) {
                            /*
                            GameObject prefabParent = source.AbilityManager.UnitGameObject;
                            Transform usedPrefabSourceBone = null;
                            if (prefabProfile.TargetBone != null && prefabProfile.TargetBone != string.Empty) {
                                usedPrefabSourceBone = prefabParent.transform.FindChildByRecursive(prefabProfile.TargetBone);
                            }
                            if (usedPrefabSourceBone != null) {
                                prefabParent = usedPrefabSourceBone.gameObject;
                            }
                            */
                            Vector3 endPosition = Vector3.zero;
                            Interactable usedTarget = target;
                            if (abilityEffectContext.baseAbility != null && abilityEffectContext.baseAbility.GetTargetOptions(source).RequiresGroundTarget == true) {
                                endPosition = abilityEffectContext.groundTargetLocation;
                                usedTarget = null;
                                //Debug.Log(DisplayName + "ChanneledEffect.Cast() abilityEffectInput.prefabLocation: " + abilityEffectInput.prefabLocation);
                            } else {
                                endPosition = target.GetComponent<Collider>().bounds.center - target.transform.position;
                            }

                            channeledObjectScript.Setup(prefabParent, sourcePosition, usedTarget?.gameObject, endPosition, systemGameManager);
                            //channeledObjectScript.MyStartObject = prefabParent;
                            //channeledObjectScript.MyStartPosition = source.AbilityManager.UnitGameObject.GetComponent<Collider>().bounds.center - source.MyCharacterUnit.transform.position;
                            //channeledObjectScript.MyStartPosition = prefabProfile.MyPosition;
                            //channeledObjectScript.MyStartPosition = prefabParent.transform.TransformPoint(prefabOffset);
                            //channeledObjectScript.MyEndObject = target.gameObject;
                            //channeledObjectScript.MyEndPosition = target.GetComponent<Collider>().bounds.center - target.transform.position;
                        } else {
                            Debug.LogError(DisplayName + ".ChanneledEffect.Cast(" + source + ", " + (target == null ? "null" : target.name) + "): CHECK INSPECTOR, IChanneledObject NOT FOUND");
                        }
                    }
                   
                }

                // delayed damage
                //source.StartCoroutine(PerformAbilityHitDelay(source, target, abilityEffectInput));
                source.AbilityManager.BeginPerformAbilityHitDelay(source, target, abilityEffectContext, this);
            } else {
                //Debug.Log(DisplayName + ".ChanneledEffect.Cast(" + source + ", " + (target == null ? "null" : target.name) + ") PREFABOBJECTS WAS NULL");

            }
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
