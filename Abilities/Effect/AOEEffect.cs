using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New AOEEffect", menuName = "AnyRPG/Abilities/Effects/AOEEffect")]
    public class AOEEffect : FixedLengthEffect {

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

        // delay between casting hit effect on each target
        [SerializeField]
        protected float interTargetDelay = 0f;

        /// <summary>
        /// Does the actual work of hitting the target with an ability
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public override Dictionary<PrefabProfile, GameObject> Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".AOEEffect.Cast(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            if (abilityEffectInput == null) {
                abilityEffectInput = new AbilityEffectOutput();
            }
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
            TargetAOEHit(source, target, abilityEffectInput);
            return returnObjects;
        }

        public override void CastTick(BaseCharacter source, GameObject target, AbilityEffectOutput abilityAffectInput) {
            //Debug.Log(resourceName + ".AOEEffect.CastTick(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            TargetAOETick(source, target, abilityAffectInput);
        }

        public override void CastComplete(BaseCharacter source, GameObject target, AbilityEffectOutput abilityAffectInput) {
            //Debug.Log(resourceName + ".AOEEffect.CastComplete(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            TargetAOEComplete(source, target, abilityAffectInput);
        }

        protected virtual float TargetAOEHit(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + "AOEEffect.TargetAOEHit(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            List<AOETargetNode> validTargets = GetValidTargets(source, target, abilityEffectInput, hitAbilityEffectList);
            float accumulatedDelay = 0f;
            foreach (AOETargetNode validTarget in validTargets) {
                PerformAOEHit(source, validTarget.targetGameObject, 1f / validTargets.Count, validTarget.abilityEffectInput, accumulatedDelay);
                accumulatedDelay += interTargetDelay;
            }
            return validTargets.Count;
        }

        protected virtual float TargetAOETick(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            List<AOETargetNode> validTargets = GetValidTargets(source, target, abilityEffectInput, tickAbilityEffectList);
            float accumulatedDelay = 0f;
            foreach (AOETargetNode validTarget in validTargets) {
                PerformAOETick(source, validTarget.targetGameObject, 1f / validTargets.Count, validTarget.abilityEffectInput, accumulatedDelay);
                accumulatedDelay += interTargetDelay;
            }
            return validTargets.Count;
        }

        protected virtual float TargetAOEComplete(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
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

        protected virtual List<AOETargetNode> GetValidTargets(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput, List<AbilityEffect> abilityEffectList) {
            //Debug.Log(MyName + ".AOEEffect.GetValidTargets()");

            Vector3 aoeSpawnCenter = Vector3.zero;
            if (prefabSpawnLocation == PrefabSpawnLocation.Target && target != null) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to target");
                aoeSpawnCenter = target.transform.position;
            } else if (prefabSpawnLocation == PrefabSpawnLocation.Caster) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to caster");
                aoeSpawnCenter = source.MyCharacterUnit.transform.position;
                aoeSpawnCenter += source.MyCharacterUnit.transform.TransformDirection(aoeCenter);
            } else if (prefabSpawnLocation == PrefabSpawnLocation.Point) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to groundTarget at: " + abilityEffectInput.prefabLocation);
                aoeSpawnCenter = abilityEffectInput.prefabLocation;
                aoeSpawnCenter += aoeCenter;
            } else {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to vector3.zero!!! was prefab spawn location not set or target despawned?");
            }
            //aoeSpawnCenter += source.MyCharacterUnit.transform.TransformDirection(aoeCenter);
            Collider[] colliders = new Collider[0];
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            int validMask = (playerMask | characterMask);
            if (useRadius) {
                colliders = Physics.OverlapSphere(aoeSpawnCenter, aoeRadius, validMask);
            }
            if (useExtents) {
                //Debug.Log(MyName + ".AOEEffect.GetValidTargets(): using aoeSpawnCenter: " + aoeSpawnCenter + ", extents: " + aoeExtents);
                colliders = Physics.OverlapBox(aoeSpawnCenter, aoeExtents / 2f, source.MyCharacterUnit.transform.rotation, validMask);
            }
            //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
            List<AOETargetNode> validTargets = new List<AOETargetNode>();
            foreach (Collider collider in colliders) {
                //Debug.Log("AOEEffect.Cast() hit: " + collider.gameObject.name + "; " + collider.gameObject.layer);
                bool canAdd = true;
                foreach (AbilityEffect abilityEffect in abilityEffectList) {
                    if (abilityEffect.CanUseOn(collider.gameObject, source) == false) {
                        canAdd = false;
                    }
                }
                //if (CanUseOn(collider.gameObject, source)) {
                // next line was preventing aoe from hitting current target
                //if (collider.gameObject != target && CanUseOn(collider.gameObject, source)) {
                //Debug.Log("performing AOE ability: " + MyAbilityEffectName + " on " + collider.gameObject);
                if (canAdd) {
                    AOETargetNode validTargetNode = new AOETargetNode();
                    validTargetNode.targetGameObject = collider.gameObject;
                    validTargetNode.abilityEffectInput = abilityEffectInput;
                    validTargets.Add(validTargetNode);
                }
                if (maxTargets > 0) {
                    //Debug.Log("AOEEffect.GetValidTargets(). maxTargets: " + maxTargets + "; validTargets.Count: " + validTargets.Count);
                    while (validTargets.Count > maxTargets) {
                        int randomNumber = Random.Range(0, validTargets.Count);
                        //Debug.Log("AOEEffect.GetValidTargets(). maxTargets: " + maxTargets + "; validTargets.Count: " + validTargets.Count + "; randomNumber: " + randomNumber);
                        validTargets.RemoveAt(randomNumber);
                    }
                }
                //}
            }
            //Debug.Log(abilityEffectName + ".AOEEffect.Cast(). Valid targets count: " + validTargets.Count);
            return validTargets;
        }

        protected virtual AbilityEffectOutput GetSharedOutput(float outputShare, AbilityEffectOutput abilityEffectInput) {
            AbilityEffectOutput modifiedOutput = new AbilityEffectOutput();
            modifiedOutput.healthAmount = (int)(abilityEffectInput.healthAmount * outputShare);
            modifiedOutput.manaAmount = (int)(abilityEffectInput.manaAmount * outputShare);
            modifiedOutput.prefabLocation = abilityEffectInput.prefabLocation;
            return modifiedOutput;
        }

        public virtual void PerformAOEHit(BaseCharacter source, GameObject target, float outputShare, AbilityEffectOutput abilityEffectInput, float castDelay) {
            //Debug.Log(MyName + ".AOEEffect.PerformAOEHit(): abilityEffectInput.healthAmount: " + (abilityEffectInput == null ? "null" : abilityEffectInput.healthAmount.ToString()) + "; outputShare: " + outputShare);
            AbilityEffectOutput modifiedOutput = GetSharedOutput(outputShare, abilityEffectInput);
            source.StartCoroutine(WaitForHitDelay(source, target, modifiedOutput, castDelay));
        }

        private IEnumerator WaitForHitDelay(BaseCharacter source, GameObject target, AbilityEffectOutput modifiedOutput, float castDelay) {
            float accumulatedTime = 0f;
            while (accumulatedTime < castDelay) {
                accumulatedTime += Time.deltaTime;
                yield return null;
            }
            PerformAbilityHit(source, target, modifiedOutput);
        }

        public virtual void PerformAOETick(BaseCharacter source, GameObject target, float outputShare, AbilityEffectOutput abilityEffectInput, float castDelay) {
            //Debug.Log(resourceName + ".AOEEffect.PerformAbilityEffect(): abilityEffectInput.healthAmount: " + (abilityEffectInput == null ? "null" : abilityEffectInput.healthAmount.ToString()) + "; outputShare: " + outputShare);
            AbilityEffectOutput modifiedOutput = GetSharedOutput(outputShare, abilityEffectInput);
            source.StartCoroutine(WaitForTickDelay(source, target, modifiedOutput, castDelay));
        }

        private IEnumerator WaitForTickDelay(BaseCharacter source, GameObject target, AbilityEffectOutput modifiedOutput, float castDelay) {
            float accumulatedTime = 0f;
            while (accumulatedTime < castDelay) {
                accumulatedTime += Time.deltaTime;
                yield return null;
            }
            PerformAbilityTick(source, target, modifiedOutput);
        }

        public virtual void PerformAOEComplete(BaseCharacter source, GameObject target, float outputShare, AbilityEffectOutput abilityEffectInput, float castDelay) {
            //Debug.Log(abilityEffectName + ".AOEEffect.PerformAbilityEffect(): abilityEffectInput.healthAmount: " + (abilityEffectInput == null ? "null" : abilityEffectInput.healthAmount.ToString()) + "; outputShare: " + outputShare);
            AbilityEffectOutput modifiedOutput = GetSharedOutput(outputShare, abilityEffectInput);
            source.StartCoroutine(WaitForCompleteDelay(source, target, modifiedOutput, castDelay));
        }

        private IEnumerator WaitForCompleteDelay(BaseCharacter source, GameObject target, AbilityEffectOutput modifiedOutput, float castDelay) {
            float accumulatedTime = 0f;
            while (accumulatedTime < castDelay) {
                accumulatedTime += Time.deltaTime;
                yield return null;
            }
            PerformAbilityComplete(source, target, modifiedOutput);
        }

    }

    public class AOETargetNode {
        public GameObject targetGameObject;
        public AbilityEffectOutput abilityEffectInput;
    }

}