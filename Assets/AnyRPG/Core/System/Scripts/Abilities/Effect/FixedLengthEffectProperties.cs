using AnyRPG;
using FishNet.CodeAnalysis.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Ability",menuName = "AnyRPG/Abilities/Effects/FixedLengthEffect")]
    // not using that for now as it will neither tick, nor complete.  that is done by directeffect/children or aoeEffect
    public abstract class FixedLengthEffectProperties : LengthEffectProperties {

        [Header("Prefab")]

        [Tooltip("Ability: use ability prefabs, Both: use weapon and ability prefabs, Weapon: use only weapon prefabs")]
        [SerializeField]
        protected AbilityPrefabSource abilityPrefabSource = AbilityPrefabSource.Ability;

        //[SerializeField]
        //private List<string> prefabNames = new List<string>();

        [Tooltip("randomly select a prefab instead of spawning all of them")]
        [SerializeField]
        protected bool randomPrefabs = false;

        [Tooltip("Prefabs to spawn when this effect is cast")]
        [SerializeField]
        private List<AbilityAttachmentNode> abilityObjectList = new List<AbilityAttachmentNode>();

        //private List<PrefabProfile> prefabProfileList = new List<PrefabProfile>();

        [SerializeField]
        protected PrefabSpawnLocation prefabSpawnLocation;

        [Tooltip("a delay after the effect ends to destroy the spell effect prefab")]
        [SerializeField]
        protected float prefabDestroyDelay = 0f;

        [Tooltip("If true, the prefab will be destroyed when casting ends, regardless of prefab lifetime")]
        [SerializeField]
        protected bool destroyOnEndCast = false;

        /// <summary>
        /// the default amount of time after which we destroy any spawned prefab
        /// </summary>
        public float defaultPrefabLifetime = 10f;

        // game manager references
        protected SystemAbilityController systemAbilityController = null;

        public float PrefabDestroyDelay { get => prefabDestroyDelay; set => prefabDestroyDelay = value; }
        public PrefabSpawnLocation PrefabSpawnLocation { get => prefabSpawnLocation; set => prefabSpawnLocation = value; }
        public bool DestroyOnEndCast { get => destroyOnEndCast; set => destroyOnEndCast = value; }
        public bool RandomPrefabs { get => randomPrefabs; set => randomPrefabs = value; }
        public List<AbilityAttachmentNode> AbilityObjectList { get => abilityObjectList; set => abilityObjectList = value; }
        public float AbilityEffectObjectLifetime { get => defaultPrefabLifetime; set => defaultPrefabLifetime = value; }

        /*
        public void GetFixedLengthEffectProperties(FixedLengthEffect effect) {

            defaultPrefabLifetime = effect.defaultPrefabLifetime;

            GetLengthEffectProperties(effect);
        }
        */

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

        public List<AbilityAttachmentNode> GetPrefabProfileList(IAbilityCaster abilityCaster) {
            if (abilityPrefabSource == AbilityPrefabSource.Both) {
                List<AbilityAttachmentNode> returnList = new List<AbilityAttachmentNode>();
                returnList.AddRange(abilityObjectList);
                foreach (AbilityAttachmentNode abilityAttachmentNode in abilityCaster.AbilityManager.GetWeaponAbilityObjectList()) {
                    if (abilityAttachmentNode.HoldableObject != null) {
                        returnList.Add(abilityAttachmentNode);
                    }
                }
                return returnList;
            }
            if (abilityPrefabSource == AbilityPrefabSource.Weapon) {
                List<AbilityAttachmentNode> returnList = new List<AbilityAttachmentNode>();
                foreach (AbilityAttachmentNode abilityAttachmentNode in abilityCaster.AbilityManager.GetWeaponAbilityObjectList()) {
                    if (abilityAttachmentNode.HoldableObject != null) {
                        returnList.Add(abilityAttachmentNode);
                    }
                }
                return returnList;
            }

            // abilityPrefabSource is AbilityPrefabSource.Ability since there are only 3 options
            return abilityObjectList;

        }

        public override Dictionary<PrefabProfile, List<GameObject>> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(DisplayName + ".LengthEffect.Cast(" + (source == null ? "null" : source.AbilityManager.Name) + ", " + (target == null ? "null" : target.gameObject.name) + ", " + (originalTarget == null ? "null" : originalTarget.name) + ")");

            //Dictionary<PrefabProfile, List<GameObject>> prefabObjects = base.Cast(source, target, originalTarget, abilityEffectInput);

            if (networkManagerServer.ServerModeActive == true && ((source is UnitController) == false) && target is IAbilityCaster) {
                // effects cast by environmental areas will spawn on network clients this way
                return (target as IAbilityCaster).AbilityManager.SpawnAbilityEffectPrefabs(target, originalTarget, this, abilityEffectInput);
            } else {
                return source.AbilityManager.SpawnAbilityEffectPrefabs(target, originalTarget, this, abilityEffectInput);
            }
            //Dictionary<PrefabProfile, List<GameObject>> prefabObjects = source.AbilityManager.SpawnAbilityEffectPrefabs(target, originalTarget, this, abilityEffectInput);
            //abilityEffectInput.PrefabObjects = prefabObjects;
            //return prefabObjects;
        }


        public override void BeginMonitoring(Dictionary<PrefabProfile, List<GameObject>> abilityEffectObjects, IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log($"{ResourceName}.FixedLengthEffect.BeginMonitoring({(abilityEffectObjects == null ? "null" : abilityEffectObjects.Count.ToString())}, {(target == null ? "null" : target.name)})");
            
            base.BeginMonitoring(abilityEffectObjects, source, target, abilityEffectInput);
            //Debug.Log("FixedLengthEffect.BeginMonitoring(); source: " + source.name);
            //source.StartCoroutine(DestroyAbilityEffectObject(abilityEffectObject, source, target, defaultPrefabLifetime, abilityEffectInput));
            CheckDestroyObjects(abilityEffectObjects, source, target, abilityEffectInput);
        }

        protected virtual void CheckDestroyObjects(Dictionary<PrefabProfile, List<GameObject>> abilityEffectObjects, IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(DisplayName + ".FixedLengthEffect.CheckDestroyObjects(" + (abilityEffectObjects == null ? "null" : abilityEffectObjects.Count.ToString()) + ", " + (source == null ? "null" : source.AbilityManager.Name) + ", " + (target == null ? "null" : target.name) + ")");
            if (source != null) {
                systemAbilityController.BeginDestroyAbilityEffectObject(abilityEffectObjects, source, target, defaultPrefabLifetime, abilityEffectInput, this);
            }
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {
            base.SetupScriptableObjects(systemGameManager, describable);

            if (abilityObjectList != null) {
                foreach (AbilityAttachmentNode abilityAttachmentNode in abilityObjectList) {
                    if (abilityAttachmentNode != null) {
                        abilityAttachmentNode.SetupScriptableObjects(DisplayName, systemGameManager);
                    }
                }
            }

        }

    }
}