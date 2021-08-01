using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class EnvironmentalEffectArea : MonoBehaviour, IAbilityCaster {

        [Tooltip("Every x seconds, the effect will be applied to everyone within the effect radius")]
        [SerializeField]
        private float tickRate = 1f;

        [Tooltip("The name of the ability effect to cast on valid targets every tick")]
        [SerializeField]
        private List<string> abilityEffectNames = new List<string>();

        // a reference to the ability effect to apply to targets on tick
        private List<AbilityEffect> abilityEffects = new List<AbilityEffect>();

        // a counter to keep track of the amount of time passed since the last tick
        private float elapsedTime = 0f;

        private BoxCollider boxCollider = null;

        private AbilityManager abilityManager = null;

        public IAbilityManager AbilityManager { get => abilityManager; }

        private void Awake() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.Awake()");
            GetComponentReferences();
            SetupScriptableObjects();
            abilityManager = new AbilityManager(this);
        }

        public void GetComponentReferences() {
            boxCollider = GetComponent<BoxCollider>();
        }


        private void FixedUpdate() {
            elapsedTime += Time.fixedDeltaTime;
            if (elapsedTime > tickRate) {
                //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.FixedUpdate()");
                elapsedTime -= tickRate;
                PerformAbilityEffects();
            }
        }

        private void PerformAbilityEffects() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.PerformAbilityEffects()");

            List<AOETargetNode> validTargets = GetValidTargets();
            foreach (AOETargetNode validTarget in validTargets) {
                foreach (AbilityEffect abilityEffect in abilityEffects) {
                    //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.PerformAbilityEffects(): casting " + abilityEffect.MyName);

                    abilityEffect.Cast(this, validTarget.targetGameObject, null, new AbilityEffectContext());
                }
            }
        }

        protected virtual List<AOETargetNode> GetValidTargets() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.GetValidTargets()");

            Vector3 aoeSpawnCenter = transform.position;

            Collider[] colliders = new Collider[0];
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            int validMask = (playerMask | characterMask);

            //Debug.Log(MyName + ".AOEEffect.GetValidTargets(): using aoeSpawnCenter: " + aoeSpawnCenter + ", extents: " + aoeExtents);
            colliders = Physics.OverlapBox(aoeSpawnCenter, boxCollider.bounds.extents, Quaternion.identity, validMask);

            //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
            List<AOETargetNode> validTargets = new List<AOETargetNode>();
            foreach (Collider collider in colliders) {
                //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.GetValidTargets() hit: " + collider.gameObject.name + "; layer: " + collider.gameObject.layer);

                bool canAdd = true;
                Interactable interactable = collider.gameObject.GetComponent<Interactable>();
                if (interactable == null) {
                    canAdd = false;
                } else {
                    if (CharacterUnit.GetCharacterUnit(interactable) == null) {
                        canAdd = false;
                    }
                }
                if (canAdd) {
                    AOETargetNode validTargetNode = new AOETargetNode();
                    validTargetNode.targetGameObject = interactable;
                    validTargets.Add(validTargetNode);
                }
            }
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.GetValidTargets(). Valid targets count: " + validTargets.Count);
            return validTargets;
        }

        private void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.SetupScriptableObjects()");
            if (SystemGameManager.Instance == null) {
                Debug.LogError(gameObject.name + ": SystemAbilityEffectManager not found.  Is the GameManager in the scene?");
                return;
            }

            if (abilityEffectNames != null) {
                foreach (string abilityEffectName in abilityEffectNames) {
                    if (abilityEffectName != string.Empty) {
                        AbilityEffect tmpAbilityEffect = SystemDataFactory.Instance.GetResource<AbilityEffect>(abilityEffectName);
                        if (tmpAbilityEffect != null) {
                            abilityEffects.Add(tmpAbilityEffect);
                        } else {
                            Debug.LogError(gameObject.name + ".EnvironmentalEffectArea.SetupScriptableObjects(): Could not find ability effect " + abilityEffectName + " while initializing " + gameObject.name + ". Check inspector.");
                        }
                    } else {
                        Debug.LogError(gameObject.name + ".EnvironmentalEffectArea.SetupScriptableObjects(): Ability Effect name was empty while initializing " + gameObject.name + ". Check inspector.");
                    }
                }
            }
        }

    }

}
