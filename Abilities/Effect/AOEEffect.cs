using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New AOEEffect", menuName = "AnyRPG/Abilities/Effects/AOEEffect")]
    public class AOEEffect : FixedLengthEffect {

        [Header("AOE")]

        [SerializeField]
        protected float aoeRadius;

        [SerializeField]
        protected bool useRadius = true;

        [SerializeField]
        protected bool useExtents = false;

        [SerializeField]
        protected Vector3 aoeCenter;

        [SerializeField]
        protected Vector3 aoeExtents;

        [SerializeField]
        protected float maxTargets = 0;

        [SerializeField]
        protected bool preferClosestTargets = false;

        // delay between casting hit effect on each target
        [SerializeField]
        protected float interTargetDelay = 0f;

        /// <summary>
        /// Does the actual work of hitting the target with an ability
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(MyName + ".AOEEffect.Cast(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            if (abilityEffectContext == null) {
                abilityEffectContext = new AbilityEffectContext();
            }
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectContext);
            TargetAOEHit(source, target, abilityEffectContext);

            // ground targeted spells should play the audio on the prefab object, not the character
            if (prefabSpawnLocation == PrefabSpawnLocation.GroundTarget) {
                base.PlayAudioEffects(onHitAudioProfiles, null);
            } else {
                base.PlayAudioEffects(onHitAudioProfiles, target);
            }

            return returnObjects;
        }

        public override void CastTick(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(resourceName + ".AOEEffect.CastTick(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            base.CastTick(source, target, abilityEffectContext);
            TargetAOETick(source, target, abilityEffectContext);
        }

        public override void CastComplete(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(resourceName + ".AOEEffect.CastComplete(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            TargetAOEComplete(source, target, abilityEffectContext);
        }

        protected virtual float TargetAOEHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(MyName + "AOEEffect.TargetAOEHit(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            List<AOETargetNode> validTargets = GetValidTargets(source, target, abilityEffectContext, hitAbilityEffectList);
            float accumulatedDelay = 0f;
            foreach (AOETargetNode validTarget in validTargets) {
                PerformAOEHit(source, validTarget.targetGameObject, 1f / validTargets.Count, validTarget.abilityEffectInput, accumulatedDelay);
                accumulatedDelay += interTargetDelay;
            }
            return validTargets.Count;
        }

        protected virtual float TargetAOETick(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            List<AOETargetNode> validTargets = GetValidTargets(source, target, abilityEffectInput, tickAbilityEffectList);
            float accumulatedDelay = 0f;
            foreach (AOETargetNode validTarget in validTargets) {
                PerformAOETick(source, validTarget.targetGameObject, 1f / validTargets.Count, validTarget.abilityEffectInput, accumulatedDelay);
                accumulatedDelay += interTargetDelay;
            }
            return validTargets.Count;
        }

        protected virtual float TargetAOEComplete(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(MyName + "AOEEffect.TargetAOEComplete(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            if (completeAbilityEffectList == null | completeAbilityEffectList.Count == 0) {
                return 0;
            }
            List<AOETargetNode> validTargets = GetValidTargets(source, target, abilityEffectInput, completeAbilityEffectList);
            float accumulatedDelay = 0f;
            foreach (AOETargetNode validTarget in validTargets) {
                PerformAOEComplete(source, validTarget.targetGameObject, 1f / validTargets.Count, validTarget.abilityEffectInput, accumulatedDelay);
                accumulatedDelay += interTargetDelay;
            }
            return validTargets.Count;
        }

        protected virtual List<AOETargetNode> GetValidTargets(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext, List<AbilityEffect> abilityEffectList) {
            //Debug.Log(MyName + ".AOEEffect.GetValidTargets()");

            Vector3 aoeSpawnCenter = Vector3.zero;
            if (prefabSpawnLocation == PrefabSpawnLocation.Target && target != null) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to target");
                aoeSpawnCenter = target.transform.position;
            } else if (prefabSpawnLocation == PrefabSpawnLocation.Caster) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to caster");
                aoeSpawnCenter = source.AbilityManager.UnitGameObject.transform.position;
                aoeSpawnCenter += source.AbilityManager.UnitGameObject.transform.TransformDirection(aoeCenter);
            } else if (prefabSpawnLocation == PrefabSpawnLocation.GroundTarget) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to groundTarget at: " + abilityEffectInput.prefabLocation);
                aoeSpawnCenter = abilityEffectContext.groundTargetLocation;
                aoeSpawnCenter += aoeCenter;
            } else {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to vector3.zero!!! was prefab spawn location not set or target despawned?");
            }
            //aoeSpawnCenter += source.AbilityManager.UnitGameObject.transform.TransformDirection(aoeCenter);
            Collider[] colliders = new Collider[0];
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            int validMask = (playerMask | characterMask);
            if (useRadius) {
                colliders = Physics.OverlapSphere(aoeSpawnCenter, aoeRadius, validMask);
            }
            if (useExtents) {
                //Debug.Log(MyName + ".AOEEffect.GetValidTargets(): using aoeSpawnCenter: " + aoeSpawnCenter + ", extents: " + aoeExtents);
                colliders = Physics.OverlapBox(aoeSpawnCenter, aoeExtents / 2f, source.AbilityManager.UnitGameObject.transform.rotation, validMask);
            }
            //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
            List<AOETargetNode> validTargets = new List<AOETargetNode>();
            foreach (Collider collider in colliders) {
                //Debug.Log(MyName + "AOEEffect.Cast() hit: " + collider.gameObject.name + "; layer: " + collider.gameObject.layer);
                bool canAdd = true;
                Interactable targetInteractable = collider.gameObject.GetComponent<Interactable>();
                foreach (AbilityEffect abilityEffect in abilityEffectList) {
                    if (abilityEffect.CanUseOn(targetInteractable, source, abilityEffectContext) == false) {
                        canAdd = false;
                    }
                }
                //if (CanUseOn(collider.gameObject, source)) {
                // next line was preventing aoe from hitting current target
                //if (collider.gameObject != target && CanUseOn(collider.gameObject, source)) {
                //Debug.Log(MyName + "performing AOE ability  on " + collider.gameObject);
                if (canAdd) {
                    AOETargetNode validTargetNode = new AOETargetNode();
                    validTargetNode.targetGameObject = targetInteractable;
                    validTargetNode.abilityEffectInput = abilityEffectContext;
                    validTargets.Add(validTargetNode);
                }
                //Debug.Log(MyName + "AOEEffect.GetValidTargets(). maxTargets: " + maxTargets + "; validTargets.Count: " + validTargets.Count);
                if (maxTargets > 0) {
                    //Debug.Log(MyName + "AOEEffect.GetValidTargets(). maxTargets: " + maxTargets + "; validTargets.Count: " + validTargets.Count);
                    while (validTargets.Count > maxTargets) {
                        int removeNumber = 0;
                        if (preferClosestTargets == true) {
                            int counter = 0;
                            foreach (AOETargetNode validTarget in validTargets) {
                                if (Vector3.Distance(validTarget.targetGameObject.transform.position, source.AbilityManager.UnitGameObject.transform.position) > Vector3.Distance(validTargets[removeNumber].targetGameObject.transform.position, source.AbilityManager.UnitGameObject.transform.position)) {
                                    removeNumber = counter;
                                }
                                counter++;
                            }
                        } else {
                            removeNumber = Random.Range(0, validTargets.Count);
                        }
                        //Debug.Log("AOEEffect.GetValidTargets(). maxTargets: " + maxTargets + "; validTargets.Count: " + validTargets.Count + "; randomNumber: " + randomNumber);
                        validTargets.RemoveAt(removeNumber);
                    }
                }
                //}
            }
            //Debug.Log(abilityEffectName + ".AOEEffect.Cast(). Valid targets count: " + validTargets.Count);
            return validTargets;
        }

        protected virtual AbilityEffectContext GetSharedOutput(float outputShare, AbilityEffectContext abilityEffectInput) {
            AbilityEffectContext modifiedOutput = new AbilityEffectContext();

            foreach (ResourceInputAmountNode resourceInputAmountNode in abilityEffectInput.resourceAmounts) {
                modifiedOutput.AddResourceAmount(resourceInputAmountNode.resourceName, (int)(resourceInputAmountNode.amount * outputShare));
            }

            modifiedOutput.groundTargetLocation = abilityEffectInput.groundTargetLocation;
            return modifiedOutput;
        }

        public virtual void PerformAOEHit(IAbilityCaster source, Interactable target, float outputShare, AbilityEffectContext abilityEffectInput, float castDelay) {
            //Debug.Log(MyName + ".AOEEffect.PerformAOEHit(): outputShare: " + outputShare);
            AbilityEffectContext modifiedOutput = GetSharedOutput(outputShare, abilityEffectInput);
            (source as MonoBehaviour).StartCoroutine(WaitForHitDelay(source, target, modifiedOutput, castDelay));
        }

        private IEnumerator WaitForHitDelay(IAbilityCaster source, Interactable target, AbilityEffectContext modifiedOutput, float castDelay) {
            //Debug.Log(MyName + ".AOEEffect.WaitForHitDelay(" + source.MyName + ", " + (target == null ? "null" : target.name) + ")");
            float accumulatedTime = 0f;
            while (accumulatedTime < castDelay) {
                yield return null;
                accumulatedTime += Time.deltaTime;
            }
            PerformAbilityHit(source, target, modifiedOutput);
        }

        public virtual void PerformAOETick(IAbilityCaster source, Interactable target, float outputShare, AbilityEffectContext abilityEffectInput, float castDelay) {
            //Debug.Log(resourceName + ".AOEEffect.PerformAbilityEffect(): outputShare: " + outputShare);
            AbilityEffectContext modifiedOutput = GetSharedOutput(outputShare, abilityEffectInput);
            (source as MonoBehaviour).StartCoroutine(WaitForTickDelay(source, target, modifiedOutput, castDelay));
        }

        private IEnumerator WaitForTickDelay(IAbilityCaster source, Interactable target, AbilityEffectContext modifiedOutput, float castDelay) {
            float accumulatedTime = 0f;
            while (accumulatedTime < castDelay) {
                yield return null;
                accumulatedTime += Time.deltaTime;
            }
            PerformAbilityTick(source, target, modifiedOutput);
        }

        public virtual void PerformAOEComplete(IAbilityCaster source, Interactable target, float outputShare, AbilityEffectContext abilityEffectInput, float castDelay) {
            //Debug.Log(abilityEffectName + ".AOEEffect.PerformAbilityEffect(): outputShare: " + outputShare);
            AbilityEffectContext modifiedOutput = GetSharedOutput(outputShare, abilityEffectInput);
            (source as MonoBehaviour).StartCoroutine(WaitForCompleteDelay(source, target, modifiedOutput, castDelay));
        }

        private IEnumerator WaitForCompleteDelay(IAbilityCaster source, Interactable target, AbilityEffectContext modifiedOutput, float castDelay) {
            float accumulatedTime = 0f;
            while (accumulatedTime < castDelay) {
                yield return null;
                accumulatedTime += Time.deltaTime;
            }
            PerformAbilityComplete(source, target, modifiedOutput);
        }

        public override void PlayAudioEffects(List<AudioProfile> audioProfiles, Interactable target) {
            // aoe effects are special.  They are considered to have hit, whether or not they found any valid targets
            // this override prevents the audio from playing multiple times if the aoe effects multiple targets
            //base.PlayAudioEffects(audioProfiles, target);
        }

    }

    public class AOETargetNode {
        public Interactable targetGameObject;
        public AbilityEffectContext abilityEffectInput;
    }

}