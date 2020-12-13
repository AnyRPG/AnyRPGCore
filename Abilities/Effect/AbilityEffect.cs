using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {

    public abstract class AbilityEffect : DescribableResource, ITargetable {

        [Header("Target Properties")]

        [SerializeField]
        private AbilityEffectTargetProps targetOptions = new AbilityEffectTargetProps();

        /*
        [Tooltip("If true, the character must have a target selected to cast this ability.")]
        [SerializeField]
        protected bool requiresTarget = false;

        [Tooltip("If true, the character must have an uninterrupted line of sight to the target.")]
        [SerializeField]
        private bool requireLineOfSight = false;

        [Tooltip("If line of sight is required, where should it be calculated from. Useful for splash damage and ground target explosions.")]
        [SerializeField]
        protected LineOfSightSourceLocation lineOfSightSourceLocation;

        [Tooltip("If true, the target must be a character and must be alive.")]
        [SerializeField]
        protected bool requiresLiveTarget = false;

        [Tooltip("If true, the target must be a character and must be dead.")]
        [SerializeField]
        protected bool requireDeadTarget = false;

        [Tooltip("Can the character cast this ability on itself?")]
        [SerializeField]
        protected bool canCastOnSelf = false;

        [Tooltip("Can the character cast this ability on others?")]
        [SerializeField]
        protected bool canCastOnOthers = false;

        [Tooltip("Can the character cast this ability on a character belonging to an enemy faction?")]
        [SerializeField]
        protected bool canCastOnEnemy = false;

        [Tooltip("Can the character cast this ability on a character with no relationship?")]
        [SerializeField]
        protected bool canCastOnNeutral = false;

        [Tooltip("Can the character cast this ability on a character belonging to a friendly faction?")]
        [SerializeField]
        protected bool canCastOnFriendly = false;

        [Tooltip("If no target is given, automatically cast on the caster")]
        [SerializeField]
        protected bool autoSelfCast = false;

        [Header("Range Settings")]

        [Tooltip("Where to calculate max range from.  Useful for splash damage and ground target explosions.")]
        [SerializeField]
        protected TargetRangeSourceLocation targetRangeSourceLocation;

        [Tooltip("If true, the target must be within melee range (within hitbox) to cast this ability.")]
        [SerializeField]
        protected bool useMeleeRange = false;

        [Tooltip("If melee range is not used, this ability can be cast on targets this many meters away.")]
        [SerializeField]
        protected int maxRange;
        */

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
        //public bool RequireDeadTarget { get => requireDeadTarget; set => requireDeadTarget = value; }
        //public bool RequiresLiveTarget { get => requiresLiveTarget; set => requiresLiveTarget = value; }
        //public bool RequiresTarget { get => requiresTarget; set => requiresTarget = value; }
        //public int MaxRange { get => maxRange; set => maxRange = value; }
        //public bool AutoSelfCast { get => autoSelfCast; set => autoSelfCast = value; }
        //public bool CanCastOnFriendly { get => canCastOnFriendly; set => canCastOnFriendly = value; }
        //public bool CanCastOnEnemy { get => canCastOnEnemy; set => canCastOnEnemy = value; }
        //public bool CanCastOnSelf { get => canCastOnSelf; set => canCastOnSelf = value; }
        //public bool UseMeleeRange { get => useMeleeRange; set => useMeleeRange = value; }
        public IAbilityCaster SourceCharacter { get => sourceCharacter; set => sourceCharacter = value; }
        public float ThreatMultiplier { get => threatMultiplier; set => threatMultiplier = value; }
        //public bool RequireLineOfSight { get => requireLineOfSight; set => requireLineOfSight = value; }
        //public LineOfSightSourceLocation LineOfSightSourceLocation { get => lineOfSightSourceLocation; set => lineOfSightSourceLocation = value; }
        //public TargetRangeSourceLocation TargetRangeSourceLocation { get => targetRangeSourceLocation; set => targetRangeSourceLocation = value; }
        //public bool RequiresGroundTarget { get => false; }
        //public bool CanCastOnNeutral { get => canCastOnNeutral; set => canCastOnNeutral = value; }
        //public bool CanCastOnOthers { get => canCastOnOthers; set => canCastOnOthers = value; }
        public TargetProps GetTargetOptions(IAbilityCaster abilityCaster) {
            return targetOptions;
        }

        //public List<AudioClip> MyOnHitAudioClips { get => (onHitAudioProfiles == null ? null : onHitAudioProfile.MyAudioClip ); }

        public virtual void Initialize(IAbilityCaster source, BaseCharacter target, AbilityEffectContext abilityEffectInput) {
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
            //Debug.Log(abilityEffectName + ".AbilityEffect.OnDisable()");
        }

        public string GetShortDescription() {
            return description;
        }


        public virtual bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false) {
            //Debug.Log(DisplayName + ".AbilityEffect.CanUseOn(" + (target == null ? "null " : target.gameObject.name) + ", " + sourceCharacter.AbilityManager.Name + ")");

            return TargetProps.CanUseOn(this, target, sourceCharacter, abilityEffectContext, playerInitiated);
        }

        public virtual Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(DisplayName + ".AbilityEffect.Cast(" + source.AbilityManager.Name + ", " + (target? target.name : "null") + ")");
            /*
            if (abilityEffectInput != null) {
                this.abilityEffectInput = abilityEffectInput;
            }
            */
            return null;
        }

        
        public virtual Interactable ReturnTarget(IAbilityCaster sourceCharacter, Interactable target, AbilityEffectContext abilityEffectContext = null) {
            if (sourceCharacter == null || sourceCharacter.AbilityManager.UnitGameObject == null) {
                //Debug.Log("BaseAbility.ReturnTarget(): source is null! This should never happen!!!!!");
                return null;
            }

            // perform ability dependent checks
            if (CanUseOn(target, sourceCharacter, abilityEffectContext) == false) {
                //Debug.Log(DisplayName + ".BaseAbility.CanUseOn(" + (target != null ? target.name : "null") + " was false");
                if (GetTargetOptions(sourceCharacter).CanCastOnSelf && GetTargetOptions(sourceCharacter).AutoSelfCast) {
                    target = sourceCharacter.AbilityManager.UnitGameObject.GetComponent<Interactable>();
                    //Debug.Log(DisplayName + ".BaseAbility.ReturnTarget(): returning target as sourcecharacter: " + target.name);
                    return target;
                } else {
                    //Debug.Log(DisplayName + ".BaseAbility.ReturnTarget(): returning null");
                    return null;
                }
            }

            return target;
        }
        

        /// <summary>
        /// this should be done at the end of the ability
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public Dictionary<PrefabProfile, GameObject> PerformAbilityEffects(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext, List<AbilityEffect> abilityEffectList) {
            //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffects(" + source.AbilityManager.Name + ", " + (target ? target.name : "null") + ")");
            Dictionary<PrefabProfile, GameObject> returnList = new Dictionary<PrefabProfile, GameObject>();

            foreach (AbilityEffect abilityEffect in abilityEffectList) {
                if (abilityEffect != null) {
                    //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffects() found: " + (abilityEffect != null ? abilityEffect.DisplayName : "null"));
                    if (SystemResourceManager.MatchResource(abilityEffect.DisplayName, DisplayName)) {
                        Debug.LogError(DisplayName + ".PerformAbilityEffects(): circular reference detected.  Tried to cast self.  CHECK INSPECTOR AND FIX ABILITY EFFECT CONFIGURATION!!!");
                    } else {
                        if (!(abilityEffect is AmountEffect)) {
                            abilityEffectContext.spellDamageMultiplier = 1f;
                        }
                        Dictionary<PrefabProfile, GameObject> tmpObjects = PerformAbilityEffect(source, target, abilityEffectContext, abilityEffect);
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

        /// <summary>
        /// allow ability effects to chain and perform proper retargeting
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="abilityEffectContext"></param>
        /// <param name="abilityEffect"></param>
        /// <returns></returns>
        protected Dictionary<PrefabProfile, GameObject> PerformAbilityEffect(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext, AbilityEffect abilityEffect) {
            //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffect(" + source.AbilityManager.Name + ", " + (target == null ? "null" : target.name) + ", " + abilityEffect.DisplayName + ")");
            Dictionary<PrefabProfile, GameObject> returnObjects = null;
            // give the ability a chance to auto-selfcast if the original target was null

            // perform ability dependent target check
            Interactable finalTarget = abilityEffect.ReturnTarget(source, target, abilityEffectContext);

            // no longer used with targetProps
            // perform source dependent target check
            //finalTarget = source.AbilityManager.ReturnTarget(abilityEffect, target);

            //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffect(): FinalTarget: " + (finalTarget == null ? "null" : finalTarget.name));

            //if (abilityEffect.CanUseOn(finalTarget, source, abilityEffectContext)) {
            if (target != null) { 
                //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffects(): Target: " + (target == null ? "null" : target.name) + " is valid. CASTING ABILITY effect: " + abilityEffect);
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(abilityEffect.DisplayName);
                returnObjects = _abilityEffect.Cast(source, finalTarget, target, abilityEffectContext);
            } else {
                //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffects(): Target: " + (target == null ? "null" : target.name) + " is NOT VALID.");
            }
            return returnObjects;
        }

        public virtual Dictionary<PrefabProfile, GameObject> PerformAbilityHitEffects(IAbilityCaster source, Interactable target, AbilityEffectContext effectOutput) {
            //Debug.Log(MyName + ".AbilityEffect.PerformAbilityHitEffects(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
            return PerformAbilityEffects(source, target, effectOutput, hitAbilityEffectList);
        }

        public virtual void PlayAudioEffects(List<AudioProfile> audioProfiles, Interactable target) {
            //Debug.Log(DisplayName + ".AbilityEffect.PlayAudioEffects(" + (target == null ? "null" : target.name) + ")");
            if (audioProfiles != null) {
                AudioSource audioSource = null;
                if (target == null || target.UnitComponentController == null) {
                    if (prefabObjects != null && prefabObjects.Count > 0) {
                        //prefabObjects.First();
                        audioSource = prefabObjects.First().Value.GetComponent<AudioSource>();
                    }
                }
                if (audioSource != null || (target != null && target.UnitComponentController != null)) {
                    List<AudioProfile> usedAudioProfiles = new List<AudioProfile>();
                    if (randomAudioProfiles == true) {
                        usedAudioProfiles.Add(audioProfiles[UnityEngine.Random.Range(0, audioProfiles.Count)]);
                    } else {
                        usedAudioProfiles = audioProfiles;
                    }
                    foreach (AudioProfile audioProfile in usedAudioProfiles) {
                        if (audioProfile.AudioClip != null) {
                            //Debug.Log(MyName + ".AbilityEffect.PerformAbilityHit(): playing audio clip: " + audioProfile.MyAudioClip.name);
                            if (target != null && target.UnitComponentController != null) {
                                target.UnitComponentController.PlayEffect(audioProfile.AudioClip);
                            } else {
                                audioSource.PlayOneShot(audioProfile.AudioClip);
                            }
                        }
                    }
                }
            }
        }

        public virtual void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(MyName + ".AbilityEffect.PerformAbilityHit(" + source.Name + ", " + (target == null ? "null" : target.name) + ")");
            Dictionary<PrefabProfile, GameObject> effectObjects = PerformAbilityHitEffects(source, target, abilityEffectInput);
            if (target == null) {
                return;
            }
            PlayAudioEffects(onHitAudioProfiles, target);
            //PerformMaterialChange(source, target);
            PerformMaterialChange(target);
        }

        //void PerformMaterialChange(BaseCharacter source, GameObject target) {
        void PerformMaterialChange(Interactable target) {
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
                MaterialChangeController materialChangeController = target.gameObject.AddComponent<MaterialChangeController>();
                materialChangeController.Initialize(materialChangeDuration, effectMaterial);
            }
        }

        public AbilityEffectContext ApplyInputMultiplier(AbilityEffectContext abilityEffectContext) {
            //Debug.Log(MyName + ".AbilityEffect.ApplyInputMultiplier()");

            foreach (ResourceInputAmountNode resourceInputAmountNode in abilityEffectContext.resourceAmounts) {
                //Debug.Log(MyName + ".AbilityEffect.ApplyInputMultiplier(): before: " + resourceInputAmountNode.amount);
                resourceInputAmountNode.amount = (int)(resourceInputAmountNode.amount * inputMultiplier);
                //Debug.Log(MyName + ".AbilityEffect.ApplyInputMultiplier(): after: " + resourceInputAmountNode.amount);
            }

            return abilityEffectContext;
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
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + audioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (effectMaterialName != null && effectMaterialName != string.Empty) {
                effectMaterial = null;
                MaterialProfile tmpMaterialProfile = SystemMaterialProfileManager.MyInstance.GetResource(effectMaterialName);
                if (tmpMaterialProfile != null) {
                    effectMaterial = tmpMaterialProfile.MyEffectMaterial;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find material profile: " + effectMaterialName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

        }

    }

    public enum LineOfSightSourceLocation { Caster, GroundTarget, OriginalTarget }

    public enum TargetRangeSourceLocation { Caster, GroundTarget, OriginalTarget }
}