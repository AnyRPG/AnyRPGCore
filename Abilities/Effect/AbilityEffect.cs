using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {

    public abstract class AbilityEffect : DescribableResource {

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

        // require target in hitbox
        [SerializeField]
        protected bool useMeleeRange;

        // ignored if useMeleeRange is checked
        [SerializeField]
        protected int maxRange;

        [SerializeField]
        private string effectMaterialName = string.Empty;

        // a material to temporarily assign to the target we hit
        //[SerializeField]
        private Material effectMaterial;

        // the duration of the material change
        [SerializeField]
        private float materialChangeDuration = 2f;

        [SerializeField]
        protected string onHitAudioProfileName;

        [SerializeField]
        protected List<string> onHitAudioProfileNames = new List<string>();

        // whether to play all audio profiles or just one random one
        [SerializeField]
        protected bool randomAudioProfiles = false;

        //protected AudioProfile onHitAudioProfile;
        protected List<AudioProfile> onHitAudioProfiles = new List<AudioProfile>();

        /*
        [SerializeField]
        protected AudioClip OnHitAudioClip;
        */

        // any abilities to cast immediately on hit
        [SerializeField]
        protected List<string> hitAbilityEffectNames = new List<string>();

        //[SerializeField]
        protected List<AbilityEffect> hitAbilityEffectList = new List<AbilityEffect>();

        // the character that cast the spell
        protected BaseCharacter sourceCharacter;

        // pass this onto the next effect
        //protected AbilityEffectOutput abilityEffectOutput = new AbilityEffectOutput();

        // receive this from the previous effect
        //protected AbilityEffectOutput abilityEffectInput = new AbilityEffectOutput();

        // amount to multiply inputs by when adding their amount to this effect
        public float inputMultiplier = 0f;

        [SerializeField]
        protected float threatMultiplier = 1f;

        /// <summary>
        /// the reference to the gameobject spawned by this ability
        /// </summary>
        //protected GameObject abilityEffectObject = null;

        protected Dictionary<PrefabProfile, GameObject> prefabObjects = new Dictionary<PrefabProfile, GameObject>();



        public List<AbilityEffect> MyHitAbilityEffectList { get => hitAbilityEffectList; set => hitAbilityEffectList = value; }
        public bool MyRequireDeadTarget { get => requireDeadTarget; set => requireDeadTarget = value; }
        public bool MyRequiresLiveTarget { get => requiresLiveTarget; set => requiresLiveTarget = value; }
        public bool MyRequiresTarget { get => requiresTarget; set => requiresTarget = value; }
        public int MyMaxRange { get => maxRange; set => maxRange = value; }
        public bool MyAutoSelfCast { get => autoSelfCast; set => autoSelfCast = value; }
        public bool MyCanCastOnFriendly { get => canCastOnFriendly; set => canCastOnFriendly = value; }
        public bool MyCanCastOnEnemy { get => canCastOnEnemy; set => canCastOnEnemy = value; }
        public bool MyCanCastOnSelf { get => canCastOnSelf; set => canCastOnSelf = value; }
        public bool MyUseMeleeRange { get => useMeleeRange; set => useMeleeRange = value; }
        public BaseCharacter MySourceCharacter { get => sourceCharacter; set => sourceCharacter = value; }
        public float MyThreatMultiplier { get => threatMultiplier; set => threatMultiplier = value; }
        //public List<AudioClip> MyOnHitAudioClips { get => (onHitAudioProfiles == null ? null : onHitAudioProfile.MyAudioClip ); }

        public virtual void Initialize(BaseCharacter source, BaseCharacter target, AbilityEffectOutput abilityEffectInput) {
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

        // this ability exists to allow a caster to auto-self cast
        public virtual GameObject ReturnTarget(BaseCharacter sourceCharacter, GameObject target) {
            //Debug.Log("BaseAbility.ReturnTarget(" + (sourceCharacter == null ? "null" : sourceCharacter.MyName) + ", " + (target == null ? "null" : target.name) + ")");
            CharacterUnit targetCharacterUnit = null;
            if (sourceCharacter == null) {
                //Debug.Log("BaseAbility.ReturnTarget(): source is null! This should never happen!!!!!");
            }
            if (target != null) {
                targetCharacterUnit = target.GetComponent<CharacterUnit>();
                if (targetCharacterUnit != null) {
                    if (!canCastOnEnemy) {
                        if (Faction.RelationWith(targetCharacterUnit.MyCharacter, sourceCharacter) <= -1) {
                            //Debug.Log("we cannot cast this on an enemy but the target was an enemy.  set target to null");
                            target = null;
                        }
                    }
                    if (!canCastOnFriendly) {
                        if (Faction.RelationWith(targetCharacterUnit.MyCharacter, sourceCharacter) >= 0) {
                            //Debug.Log("we cannot cast this on a friendly target but the target was friendly.  set target to null");
                            target = null;
                        }
                    }
                } else {
                    //Debug.Log("target did not have a characterUnit.  set target to null");
                    target = null;
                }
            }

            // convert null target to self if possible
            if (target == null) {
                if (autoSelfCast == true) {
                    //Debug.Log("target is null and autoselfcast is true.  setting target to self");
                    target = sourceCharacter.MyCharacterUnit.gameObject;
                }
            }

            if (!canCastOnSelf && target == sourceCharacter.MyCharacterUnit.gameObject) {
                //Debug.Log("we cannot cast this on ourself but the target was ourself.  set target to null");
                target = null;
            }

            return target;
        }

        public virtual bool CanUseOn(GameObject target, BaseCharacter sourceCharacter) {
            //Debug.Log(MyName + ".AbilityEffect.CanUseOn()");
            if (requiresTarget == true) {

                if (target == null) {
                    if (CombatLogUI.MyInstance != null) {
                        CombatLogUI.MyInstance.WriteCombatMessage(MyName + " requires a target");
                    }
                    return false;
                }

                if (MyUseMeleeRange) {
                    if (!sourceCharacter.MyCharacterController.IsTargetInHitBox(target)) {
                        //Debug.Log(MyName + ".AbilityEffect.CanUseOn(): OUT OF RANGE");
                        return false;
                    }
                } else {
                    if (maxRange > 0 && Vector3.Distance(sourceCharacter.MyCharacterUnit.transform.position, target.transform.position) > maxRange) {
                        CombatLogUI.MyInstance.WriteCombatMessage(target.name + " is out of range");
                        //Debug.Log(MyName + ".AbilityEffect.CanUseOn(): OUT OF RANGE");
                        return false;
                    }
                }


                if (requiresLiveTarget || requireDeadTarget) {

                    CharacterUnit targetCharacterUnit = target.GetComponent<CharacterUnit>();
                    if (targetCharacterUnit == null) {
                        //Debug.Log(MyName + ".AbilityEffect.CanUseOn(): targetCharacterUnit is null, return false");
                        return false;
                    }

                    if (targetCharacterUnit.MyCharacter.MyCharacterStats.IsAlive == false && requiresLiveTarget) {
                        // disable for spam - dead units nearby an aoe will trigger this message
                        //Debug.Log(target.name + " is not alive and this ability effect requires a live target!");
                        return false;
                    }

                    if (targetCharacterUnit.MyCharacter.MyCharacterStats.IsAlive == true && requireDeadTarget) {
                        //Debug.Log("You cannot attack a dead target");
                        return false;
                    }

                    if (!canCastOnEnemy && !canCastOnSelf && !autoSelfCast) {
                        if (Faction.RelationWith(targetCharacterUnit.MyCharacter, sourceCharacter) <= -1) {
                            //Debug.Log("we cannot cast this on an enemy but the target was an enemy.  return false");
                            return false;
                        }
                    }

                    if (!canCastOnFriendly && !canCastOnSelf && !autoSelfCast) {
                        if (target != sourceCharacter.MyCharacterUnit.gameObject && Faction.RelationWith(targetCharacterUnit.MyCharacter, sourceCharacter) >= 0) {
                            //Debug.Log("we cannot cast this on a friendly target but the target was friendly.  return false");
                            return false;
                        }
                    }

                    if (!canCastOnSelf && target == sourceCharacter.MyCharacterUnit.gameObject) {
                        //Debug.Log("we cannot cast this on ourself but the target was ourself.  return false");
                        return false;
                    }
                }

            }

            // nothing left to prevent us from casting
            //Debug.Log(MyName + ".AbilityEffect.CanUseOn() return true");
            return true;
        }

        public virtual Dictionary<PrefabProfile, GameObject> Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".AbilityEffect.Cast(" + source.name + ", " + (target? target.name : "null") + ")");
            /*
            if (abilityEffectInput != null) {
                this.abilityEffectInput = abilityEffectInput;
            }
            */
            return null;
        }

        public virtual CharacterUnit ReturnTarget(CharacterUnit source, CharacterUnit target) {
            return target;
        }

        /// <summary>
        /// this should be done at the end of the ability
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        protected Dictionary<PrefabProfile, GameObject> PerformAbilityEffects(BaseCharacter source, GameObject target, AbilityEffectOutput effectOutput, List<AbilityEffect> abilityEffectList) {
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

        protected Dictionary<PrefabProfile, GameObject> PerformAbilityEffect(BaseCharacter source, GameObject target, AbilityEffectOutput effectOutput, AbilityEffect abilityEffect) {
            //Debug.Log("AbilityEffect.PerformAbilityEffect(" + source.MyCharacterName + ", " + (target == null ? "null" : target.name) + ", " + abilityEffect.MyName + ")");
            Dictionary<PrefabProfile, GameObject> returnObjects = null;
            // give the ability a chance to auto-selfcast if the original target was null
            GameObject finalTarget = abilityEffect.ReturnTarget(source, target);
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

        public virtual void PerformAbilityHitEffects(BaseCharacter source, GameObject target, AbilityEffectOutput effectOutput) {
            //Debug.Log(MyName + ".AbilityEffect.PerformAbilityHitEffects(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityEffects(source, target, effectOutput, hitAbilityEffectList);
        }

        public virtual void PerformAbilityHit(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".AbilityEffect.PerformAbilityHit(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityHitEffects(source, target, abilityEffectInput);
            if (onHitAudioProfiles != null) {
                AudioSource audioSource = null;
                if (target != null) {
                    audioSource = target.GetComponent<AudioSource>();
                } else {
                    if (prefabObjects != null && prefabObjects.Count > 0) {
                        //prefabObjects.First();
                        audioSource = prefabObjects.First().Value.GetComponent<AudioSource>();
                    }
                }
                if (audioSource != null) {
                    List<AudioProfile> usedAudioProfiles = new List<AudioProfile>();
                    if (randomAudioProfiles == true) {
                        usedAudioProfiles.Add(onHitAudioProfiles[UnityEngine.Random.Range(0, onHitAudioProfiles.Count)]);
                    } else {
                        usedAudioProfiles = onHitAudioProfiles;
                    }
                    foreach (AudioProfile audioProfile in usedAudioProfiles) {
                        if (audioProfile.MyAudioClip != null) {
                            //Debug.Log(MyName + ".AbilityEffect.PerformAbilityHit(): playing audio clip: " + audioProfile.MyAudioClip.name);

                            audioSource.PlayOneShot(audioProfile.MyAudioClip);
                        }
                    }
                }
                //AudioManager.MyInstance.PlayEffect(OnHitAudioClip);
            }
            PerformMaterialChange(source, target);
        }

        void PerformMaterialChange(BaseCharacter source, GameObject target) {
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
            if (onHitAudioProfileName != null && onHitAudioProfileName != string.Empty) {
                onHitAudioProfileNames.Add(onHitAudioProfileName);
            }
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