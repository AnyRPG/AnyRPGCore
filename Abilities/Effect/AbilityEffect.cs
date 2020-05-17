using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {

    public abstract class AbilityEffect : DescribableResource, ITargetable {

        [Header("Valid Target Settings")]

        [SerializeField]
        protected bool requiresTarget;

        // only applies if requiresTarget is checked
        [SerializeField]
        protected bool requiresLiveTarget;

        // for now this is not used on casting, only checked for status effect removal on revive
        [SerializeField]
        protected bool requireDeadTarget;

        [SerializeField]
        protected bool canCastOnSelf;

        [SerializeField]
        protected bool canCastOnEnemy;

        [SerializeField]
        protected bool canCastOnFriendly;

        // if no target is given, automatically cast on the caster
        [SerializeField]
        protected bool autoSelfCast;

        [Header("Range Settings")]

        // require target in hitbox
        [SerializeField]
        protected bool useMeleeRange;

        // ignored if useMeleeRange is checked
        [SerializeField]
        protected int maxRange;

        [Header("Material Changes")]

        [SerializeField]
        private string effectMaterialName = string.Empty;

        // a material to temporarily assign to the target we hit
        //[SerializeField]
        private Material effectMaterial;

        [Tooltip("The length, in seconds, that any material change (such as ice freeze) should last.")]
        [SerializeField]
        private float materialChangeDuration = 2f;

        [Header("Audio")]

        [SerializeField]
        protected List<string> onHitAudioProfileNames = new List<string>();

        [Tooltip("whether to play all audio profiles or just one random one")]
        [SerializeField]
        protected bool randomAudioProfiles = false;

        //protected AudioProfile onHitAudioProfile;
        protected List<AudioProfile> onHitAudioProfiles = new List<AudioProfile>();

        [Header("Hit")]
        [Tooltip("any abilities to cast immediately on hit")]
        [SerializeField]
        protected List<string> hitAbilityEffectNames = new List<string>();

        //[SerializeField]
        protected List<AbilityEffect> hitAbilityEffectList = new List<AbilityEffect>();

        // the character that cast the spell
        protected IAbilityCaster sourceCharacter;

        [Tooltip("amount to multiply inputs by when adding their amount to this effect")]
        public float inputMultiplier = 0f;

        [SerializeField]
        protected float threatMultiplier = 1f;

        protected Dictionary<PrefabProfile, GameObject> prefabObjects = new Dictionary<PrefabProfile, GameObject>();

        public List<AbilityEffect> MyHitAbilityEffectList { get => hitAbilityEffectList; set => hitAbilityEffectList = value; }
        public bool RequireDeadTarget { get => requireDeadTarget; set => requireDeadTarget = value; }
        public bool RequiresLiveTarget { get => requiresLiveTarget; set => requiresLiveTarget = value; }
        public bool RequiresTarget { get => requiresTarget; set => requiresTarget = value; }
        public int MaxRange { get => maxRange; set => maxRange = value; }
        public bool AutoSelfCast { get => autoSelfCast; set => autoSelfCast = value; }
        public bool CanCastOnFriendly { get => canCastOnFriendly; set => canCastOnFriendly = value; }
        public bool CanCastOnEnemy { get => canCastOnEnemy; set => canCastOnEnemy = value; }
        public bool CanCastOnSelf { get => canCastOnSelf; set => canCastOnSelf = value; }
        public bool UseMeleeRange { get => useMeleeRange; set => useMeleeRange = value; }
        public IAbilityCaster SourceCharacter { get => sourceCharacter; set => sourceCharacter = value; }
        public float ThreatMultiplier { get => threatMultiplier; set => threatMultiplier = value; }
        //public List<AudioClip> MyOnHitAudioClips { get => (onHitAudioProfiles == null ? null : onHitAudioProfile.MyAudioClip ); }

        public virtual void Initialize(IAbilityCaster source, BaseCharacter target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log("AbilityEffect.Initialize(" + source.MyCharacterName + ", " + target.MyCharacterName + ")");
            this.sourceCharacter = source;
            //this.target = target;
            /*
            if (abilityEffectPrefab != null) {
                Vector3 spawnLocation = target.MyCharacterUnit.gameObject.GetComponent<Collider>().bounds.center;
                abilityEffectObject = Instantiate(abilityEffectPrefab, spawnLocation, Quaternion.identity, target.MyCharacterUnit.gameObject.transform);
            }
            */
        }

        public virtual void OnDisable() {
            //Debug.Log(abilityEffectName + ".AbilityEffect.OnDestroy()");
        }

        

        public virtual bool CanUseOn(GameObject target, IAbilityCaster sourceCharacter) {
            //Debug.Log(MyName + ".AbilityEffect.CanUseOn()");

            // create target booleans
            bool targetIsSelf = false;
            CharacterUnit targetCharacterUnit = null;

            if (requiresTarget == false) {
                //Debug.Log("BaseAbility.CanUseOn(): target not required, returning true");
                return true;
            }

            if (target == null && autoSelfCast != true) {
                if (CombatLogUI.MyInstance != null) {
                    CombatLogUI.MyInstance.WriteCombatMessage(MyName + " requires a target");
                }
                return false;
            }

            if (target == sourceCharacter.UnitGameObject) {
                targetIsSelf = true;
            }

            if (target != null) {
                targetCharacterUnit = target.GetComponent<CharacterUnit>();
                if (targetCharacterUnit != null) {

                    if (!sourceCharacter.PerformFactionCheck(this, targetCharacterUnit, targetIsSelf)) {
                        return false;
                    }

                    // liveness checks
                    if (targetCharacterUnit.MyCharacter.CharacterStats.IsAlive == false && requiresLiveTarget == true) {
                        //Debug.Log("This ability requires a live target");
                        //CombatLogUI.MyInstance.WriteCombatMessage(resourceName + " requires a live target!");
                        return false;
                    }
                    if (targetCharacterUnit.MyCharacter.CharacterStats.IsAlive == true && requireDeadTarget == true) {
                        //Debug.Log("This ability requires a dead target");
                        //CombatLogUI.MyInstance.WriteCombatMessage(resourceName + " requires a dead target!");
                        return false;
                    }
                } else {
                    if (requiresLiveTarget == true || requireDeadTarget == true) {
                        // something that is not a character unit cannot satisfy the alive or dead conditions because it is inanimate
                        return false;
                    }
                }
            }

            if (!canCastOnSelf && targetIsSelf) {
                //Debug.Log(MyName + ": Can't cast on self. return false");
                return false;
            }

            if (target != null) {
                if (canCastOnSelf && targetIsSelf) {
                    return true;
                }

                if (!sourceCharacter.IsTargetInAbilityEffectRange(this, target)) {
                    return false;
                }
            }

            //Debug.Log(MyName + ".BaseAbility.CanUseOn(): returning true");
            return true;
        }

        public virtual Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".AbilityEffect.Cast(" + source.name + ", " + (target? target.name : "null") + ")");
            /*
            if (abilityEffectInput != null) {
                this.abilityEffectInput = abilityEffectInput;
            }
            */
            return null;
        }

        
        public virtual GameObject ReturnTarget(GameObject target) {
            return target;
        }
        

        /// <summary>
        /// this should be done at the end of the ability
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public Dictionary<PrefabProfile, GameObject> PerformAbilityEffects(IAbilityCaster source, GameObject target, AbilityEffectOutput effectOutput, List<AbilityEffect> abilityEffectList) {
            //Debug.Log(MyName + ".AbilityEffect.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + ")");
            //Debug.Log(abilityEffectName + ".AbilityEffect.PerformAbilityEffects(): effectOutput.healthAmount: " + effectOutput.healthAmount);
            Dictionary<PrefabProfile, GameObject> returnList = new Dictionary<PrefabProfile, GameObject>();

            foreach (AbilityEffect abilityEffect in abilityEffectList) {
                if (abilityEffect != null) {
                    //Debug.Log(MyName + ".AbilityEffect.PerformAbilityEffects() found: " + (abilityEffect != null ? abilityEffect.MyName : "null") + "; MyName: " + (MyName == null ? "null" : MyName));
                    if (SystemResourceManager.MatchResource(abilityEffect.MyName, MyName)) {
                        Debug.LogError(MyName + ".PerformAbilityEffects(): circular reference detected.  Tried to cast self.  CHECK INSPECTOR AND FIX ABILITY EFFECT CONFIGURATION!!!");
                    } else {
                        if (!(abilityEffect is AmountEffect)) {
                            effectOutput.spellDamageMultiplier = 1f;
                        }
                        Dictionary<PrefabProfile, GameObject> tmpObjects = PerformAbilityEffect(source, target, effectOutput, abilityEffect);
                        if (tmpObjects != null) {
                            //Debug.Log(MyName + ".PerformAbilityEffects(): ADDING GAMEOBJECT TO RETURN LIST");
                            foreach (KeyValuePair<PrefabProfile, GameObject> tmpPair in tmpObjects) {
                                returnList[tmpPair.Key] = tmpPair.Value;
                            }
                        }
                    }
                }
            }
            return returnList;
        }

        protected Dictionary<PrefabProfile, GameObject> PerformAbilityEffect(IAbilityCaster source, GameObject target, AbilityEffectOutput effectOutput, AbilityEffect abilityEffect) {
            //Debug.Log("AbilityEffect.PerformAbilityEffect(" + source.MyCharacterName + ", " + (target == null ? "null" : target.name) + ", " + abilityEffect.MyName + ")");
            Dictionary<PrefabProfile, GameObject> returnObjects = null;
            // give the ability a chance to auto-selfcast if the original target was null

            // perform ability dependent target check
            GameObject finalTarget = ReturnTarget(target);

            // perform source dependent target check
            finalTarget = source.ReturnTarget(abilityEffect, target);

            //Debug.Log("FinalTarget: " + (finalTarget == null ? "null" : finalTarget.name));

            if (abilityEffect.CanUseOn(finalTarget, source)) {
                //Debug.Log("AbilityEffect.PerformAbilityEffects(): Target: " + (target == null ? "null" : target.name) + " is valid. CASTING ABILITY effect: " + abilityEffect);
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(abilityEffect.MyName);
                returnObjects = _abilityEffect.Cast(source, finalTarget, target, effectOutput);
            } else {
                //Debug.Log("AbilityEffect.PerformAbilityEffects(): Target: " + (target == null ? "null" : target.name) + " is NOT VALID.");
            }
            return returnObjects;
        }

        public virtual void PerformAbilityHitEffects(IAbilityCaster source, GameObject target, AbilityEffectOutput effectOutput) {
            //Debug.Log(MyName + ".AbilityEffect.PerformAbilityHitEffects(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityEffects(source, target, effectOutput, hitAbilityEffectList);
        }

        public virtual void PlayAudioEffects(List<AudioProfile> audioProfiles, GameObject target) {
            if (audioProfiles != null) {
                AudioSource audioSource = null;
                UnitAudio unitAudio = null;
                if (target != null) {
                    unitAudio = target.GetComponent<UnitAudio>();
                } else {
                    if (prefabObjects != null && prefabObjects.Count > 0) {
                        //prefabObjects.First();
                        audioSource = prefabObjects.First().Value.GetComponent<AudioSource>();
                    }
                }
                if (audioSource != null || unitAudio != null) {
                    List<AudioProfile> usedAudioProfiles = new List<AudioProfile>();
                    if (randomAudioProfiles == true) {
                        usedAudioProfiles.Add(audioProfiles[UnityEngine.Random.Range(0, audioProfiles.Count)]);
                    } else {
                        usedAudioProfiles = audioProfiles;
                    }
                    foreach (AudioProfile audioProfile in usedAudioProfiles) {
                        if (audioProfile.MyAudioClip != null) {
                            //Debug.Log(MyName + ".AbilityEffect.PerformAbilityHit(): playing audio clip: " + audioProfile.MyAudioClip.name);
                            if (unitAudio != null) {
                                unitAudio.PlayEffect(audioProfile.MyAudioClip);
                            } else {
                                audioSource.PlayOneShot(audioProfile.MyAudioClip);
                            }
                        }
                    }
                }
            }
        }

        public virtual void PerformAbilityHit(IAbilityCaster source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".AbilityEffect.PerformAbilityHit(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityHitEffects(source, target, abilityEffectInput);

            PlayAudioEffects(onHitAudioProfiles, target);
            //PerformMaterialChange(source, target);
            PerformMaterialChange(target);
        }

        //void PerformMaterialChange(BaseCharacter source, GameObject target) {
        void PerformMaterialChange(GameObject target) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.PerformMaterialChange(" + source.name + ", " + target.name + ")");
            if (effectMaterial == null) {
                //Debug.Log("This effect does not have a material.  returning");
                return;
            }
            if (target == null) {
                //Debug.Log("target is null.  returning");
                return;
            }

            Renderer[] meshRenderer = target.GetComponentsInChildren<MeshRenderer>();

            if (meshRenderer == null || meshRenderer.Length == 0) {
                //Debug.Log(resourceName + ".AbilityEffect.PerformmaterialChange(): Unable to find mesh renderer in target.");
                meshRenderer = target.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (meshRenderer == null || meshRenderer.Length == 0) {
                    //Debug.Log(resourceName + ".AbilityEffect.PerformmaterialChange(): Unable to find skinned mesh renderer in target.");
                    return;
                } else {
                    //Debug.Log(resourceName + ".AbilityEffect.PerformmaterialChange(): Found " + meshRenderer.Length + " Skinned Mesh Renderers");
                }
            } else {
                //Debug.Log(resourceName + ".AbilityEffect.PerformmaterialChange(): Found " + meshRenderer.Length + " Mesh Renderers");
            }


            if (target.GetComponent<MaterialChangeController>() == null) {
                MaterialChangeController materialChangeController = target.AddComponent<MaterialChangeController>();
                materialChangeController.Initialize(materialChangeDuration, effectMaterial);
            }
        }

        public AbilityEffectOutput ApplyInputMultiplier(AbilityEffectOutput abilityEffectInput) {
            abilityEffectInput.healthAmount = (int)(abilityEffectInput.healthAmount * inputMultiplier);
            abilityEffectInput.manaAmount = (int)(abilityEffectInput.manaAmount * inputMultiplier);
            return abilityEffectInput;
        }

        public override void SetupScriptableObjects() {
            //Debug.Log(MyName + ".AbilityEffect.SetupscriptableObjects()");
            base.SetupScriptableObjects();
            hitAbilityEffectList = new List<AbilityEffect>();
            if (hitAbilityEffectNames != null) {
                foreach (string abilityEffectName in hitAbilityEffectNames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffectName);
                    if (abilityEffect != null) {
                        hitAbilityEffectList.Add(abilityEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            onHitAudioProfiles = new List<AudioProfile>();
            if (onHitAudioProfileNames != null) {
                foreach (string audioProfileName in onHitAudioProfileNames) {
                    AudioProfile audioProfile = SystemAudioProfileManager.MyInstance.GetResource(audioProfileName);
                    if (audioProfile != null) {
                        onHitAudioProfiles.Add(audioProfile);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + audioProfileName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (effectMaterialName != null && effectMaterialName != string.Empty) {
                effectMaterial = null;
                MaterialProfile tmpMaterialProfile = SystemMaterialProfileManager.MyInstance.GetResource(effectMaterialName);
                if (tmpMaterialProfile != null) {
                    effectMaterial = tmpMaterialProfile.MyEffectMaterial;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find material profile: " + effectMaterialName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

        }


    }
}