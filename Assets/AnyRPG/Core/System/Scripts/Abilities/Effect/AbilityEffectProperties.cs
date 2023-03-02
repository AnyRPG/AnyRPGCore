using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public abstract class AbilityEffectProperties : ConfiguredClass, ITargetable, IDescribable {

        [Header("Target Properties")]

        [SerializeField]
        private AbilityEffectTargetProps targetOptions = new AbilityEffectTargetProps();

        [Tooltip("The chance this ability will be cast.  100 = 100%")]
        [SerializeField]
        protected float chanceToCast = 100f;

        [Header("Material Changes")]

        [SerializeField]
        [ResourceSelector(resourceType = typeof(MaterialProfile))]
        private string effectMaterialName = string.Empty;

        // a material to temporarily assign to the target we hit
        //[SerializeField]
        private Material effectMaterial;

        [Tooltip("The length, in seconds, that any material change (such as ice freeze) should last.")]
        [SerializeField]
        private float materialChangeDuration = 2f;

        [Header("Audio")]

        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        protected List<string> onHitAudioProfileNames = new List<string>();

        [Tooltip("whether to play all audio profiles or just one random one")]
        [SerializeField]
        protected bool randomAudioProfiles = false;

        //protected AudioProfile onHitAudioProfile;
        protected List<AudioProfile> onHitAudioProfiles = new List<AudioProfile>();

        [Header("Hit")]
        [Tooltip("any abilities to cast immediately on hit")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        protected List<string> hitAbilityEffectNames = new List<string>();

        //[SerializeField]
        protected List<AbilityEffectProperties> hitAbilityEffectList = new List<AbilityEffectProperties>();

        [Tooltip("amount to multiply inputs by when adding their amount to this effect")]
        public float inputMultiplier = 0f;

        [SerializeField]
        protected float threatMultiplier = 1f;

        // properties from resource description
        /*
        protected string displayName = string.Empty;
        protected string description = string.Empty;
        protected Sprite icon = null;
        */

        protected IDescribable describableData = null;

        public List<AbilityEffectProperties> HitAbilityEffectList { get => hitAbilityEffectList; set => hitAbilityEffectList = value; }
        public float ThreatMultiplier { get => threatMultiplier; set => threatMultiplier = value; }
        public float ChanceToCast { get => chanceToCast; set => chanceToCast = value; }
        public string DisplayName { get => describableData.DisplayName; }
        public Sprite Icon { get => describableData.Icon; }
        public string Description { get => describableData.Description; }
        public AbilityEffectTargetProps TargetOptions { get => targetOptions; }

        /*
        public void GetAbilityEffectProperties(AbilityEffect effect) {

            targetOptions = effect.TargetOptions;
            chanceToCast = effect.ChanceToCast;
            effectMaterialName = effect.EffectMaterialName;
            materialChangeDuration = effect.MaterialChangeDuration;
            onHitAudioProfileNames = effect.OnHitAudioProfileNames;
            randomAudioProfiles = effect.RandomAudioProfiles;
            hitAbilityEffectNames = effect.HitAbilityEffectNames;
            inputMultiplier = effect.inputMultiplier;
            threatMultiplier = effect.ThreatMultiplier;

            //GetInstantEffectProperties(effect);
        }
        */

        public TargetProps GetTargetOptions(IAbilityCaster abilityCaster) {
            //Debug.Log(DisplayName + ".AbilityEffectProperties.GetTargetOptions()");

            return targetOptions;
        }

        public string GetShortDescription() {
            //return describableData.GetSummary();
            // ability buttons were returning the title as part of this so changed to description only
            return describableData.Description;
        }

        
        public virtual string GetName() {
            return string.Format("<color=yellow>{0}</color>", DisplayName);
        }
        

        public virtual string GetSummary() {
            return string.Format("{0}\n{1}", GetName(), GetDescription());
        }

        public virtual string GetDescription() {
            return string.Format("{0}", describableData.GetDescription());
        }

        public virtual bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            //Debug.Log(DisplayName + ".AbilityEffect.CanUseOn(" + (target == null ? "null " : target.gameObject.name) + ", " + sourceCharacter.AbilityManager.Name + ")");

            return targetOptions.CanUseOn(this, target, sourceCharacter, abilityEffectContext, playerInitiated, performRangeCheck);
        }

        public virtual Dictionary<PrefabProfile, List<GameObject>> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".AbilityEffect.Cast(" + source.AbilityManager.Name + ", " + (target? target.name : "null") + ")");
            /*
            if (abilityEffectInput != null) {
                this.abilityEffectInput = abilityEffectInput;
            }
            */
            return new Dictionary<PrefabProfile, List<GameObject>>();
        }


        public virtual Interactable ReturnTarget(IAbilityCaster sourceCharacter, Interactable target, AbilityEffectContext abilityEffectContext = null) {
            if (sourceCharacter?.AbilityManager?.UnitGameObject == null) {
                /*
                /Debug.Log(DisplayName + ".AbilityEffect.ReturnTarget(): source is null! This should never happen!!!!! (unless the thing that put the dot on you despawns)");
                if (sourceCharacter == null) {
                    Debug.Log(DisplayName + ".AbilityEffect.ReturnTarget(): source abilitymanager is null! This should never happen!!!!! " + sourceCharacter.AbilityManager.Name);
                }
                */
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
        public Dictionary<PrefabProfile, List<GameObject>> PerformAbilityEffects(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext, List<AbilityEffectProperties> abilityEffectList) {
            //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffects(" + source.AbilityManager.Name + ", " + (target ? target.name : "null") + ")");
            Dictionary<PrefabProfile, List<GameObject>> returnList = new Dictionary<PrefabProfile, List<GameObject>>();

            AbilityEffectContext abilityEffectOutput = abilityEffectContext.GetCopy();

            foreach (AbilityEffectProperties abilityEffect in abilityEffectList) {
                if (abilityEffect != null
                    && (abilityEffect.ChanceToCast >= 100f || abilityEffect.ChanceToCast >= Random.Range(0f, 100f))) {
                    //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffects() found: " + (abilityEffect != null ? abilityEffect.DisplayName : "null"));

                    // updated circular reference detection because inline ability effects will have the same name as the ability
                    //if (SystemDataFactory.MatchResource(abilityEffect.DisplayName, DisplayName)) {
                    if (abilityEffect == this) {
                        Debug.LogError(DisplayName + ".PerformAbilityEffects(): circular reference detected.  Tried to cast self.  CHECK INSPECTOR AND FIX ABILITY EFFECT CONFIGURATION!!!");
                    } else {
                        if (!(abilityEffect is AmountEffectProperties)) {
                            abilityEffectOutput.spellDamageMultiplier = 1f;
                        }
                        Dictionary<PrefabProfile, List<GameObject>> tmpObjects = PerformAbilityEffect(source, target, abilityEffectOutput, abilityEffect);
                        if (tmpObjects != null) {
                            //Debug.Log(DisplayName + ".PerformAbilityEffects(): ADDING GAMEOBJECT TO RETURN LIST");
                            foreach (KeyValuePair<PrefabProfile, List<GameObject>> tmpPair in tmpObjects) {
                                returnList[tmpPair.Key] = tmpPair.Value;
                            }
                        }
                    }
                }
            }
            return returnList;
        }

        public virtual bool CanCast() {
            return true;
        }

        /// <summary>
        /// allow ability effects to chain and perform proper retargeting
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="abilityEffectContext"></param>
        /// <param name="abilityEffect"></param>
        /// <returns></returns>
        protected Dictionary<PrefabProfile, List<GameObject>> PerformAbilityEffect(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext, AbilityEffectProperties abilityEffect) {
            //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffect(" + source.AbilityManager.Name + ", " + (target == null ? "null" : target.name) + ", " + abilityEffect.DisplayName + ")");
            Dictionary<PrefabProfile, List<GameObject>> returnObjects = null;
            // give the ability a chance to auto-selfcast if the original target was null

            // perform ability dependent target check
            Interactable finalTarget = abilityEffect.ReturnTarget(source, target, abilityEffectContext);

            // no longer used with targetProps
            // perform source dependent target check
            //finalTarget = source.AbilityManager.ReturnTarget(abilityEffect, target);

            //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffect(): FinalTarget: " + (finalTarget == null ? "null" : finalTarget.name));

            //if (abilityEffect.CanUseOn(finalTarget, source, abilityEffectContext)) {
            // null targets must be allowed for things like meteors or other projectiles that are colission based
            if (GetTargetOptions(source).RequireTarget == false || target != null) {
                //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffects(): Target: " + (target == null ? "null" : target.name) + " is valid. CASTING ABILITY effect: " + abilityEffect);
                // testing : send in copy of ability effect context so that a status effect will not remove baseability for following effects
                returnObjects = abilityEffect.Cast(source, finalTarget, target, abilityEffectContext.GetCopy());
            } else {
                //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityEffects(): Target: " + (target == null ? "null" : target.name) + " is NOT VALID.");
            }
            return returnObjects;
        }

        public virtual Dictionary<PrefabProfile, List<GameObject>> PerformAbilityHitEffects(IAbilityCaster source, Interactable target, AbilityEffectContext effectOutput) {
            //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityHitEffects(" + source.AbilityManager.Name + ", " + (target == null ? "null" : target.name) + ")");
            return PerformAbilityEffects(source, target, effectOutput, hitAbilityEffectList);
        }

        public virtual void PlayAudioEffects(List<AudioProfile> audioProfiles, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".AbilityEffect.PlayAudioEffects(" + (target == null ? "null" : target.name) + ")");
            if (audioProfiles != null) {
                AudioSource audioSource = null;
                if (target?.UnitComponentController == null) {

                    if (abilityEffectContext.PrefabObjects != null
                        && abilityEffectContext.PrefabObjects.Count > 0
                        && abilityEffectContext.PrefabObjects.First().Value.Count > 0) {
                        //prefabObjects.First();
                        audioSource = abilityEffectContext.PrefabObjects.First().Value.First().GetComponent<AudioSource>();
                        /*
                        if (audioSource != null) {
                            Debug.Log("Found Audio Source on " + abilityEffectContext.PrefabObjects.First().Value.name);
                        }
                        */
                    }
                }
                if (audioSource != null || target?.UnitComponentController != null) {
                    List<AudioProfile> usedAudioProfiles = new List<AudioProfile>();
                    if (randomAudioProfiles == true) {
                        usedAudioProfiles.Add(audioProfiles[UnityEngine.Random.Range(0, audioProfiles.Count)]);
                    } else {
                        usedAudioProfiles = audioProfiles;
                    }
                    foreach (AudioProfile audioProfile in usedAudioProfiles) {
                        if (audioProfile.AudioClips.Count > 0) {
                            //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityHit(): playing audio clip: " + audioProfile.AudioClip.name);
                            if (target != null && target.UnitComponentController != null) {
                                target.UnitComponentController.PlayEffectSound(audioProfile.RandomAudioClip);
                            } else {
                                audioSource.PlayOneShot(audioProfile.RandomAudioClip);
                            }
                        }
                    }
                }
            }
        }

        public virtual void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityHit(" + source.AbilityManager.Name + ", " + (target == null ? "null" : target.name) + ")");
            if (target == null || target != null && target.gameObject.activeSelf == true) {
                PerformAbilityHitEffects(source, target, abilityEffectContext);
            }
            if (target == null) {
                return;
            }
            PlayAudioEffects(onHitAudioProfiles, target, abilityEffectContext);
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
            //Debug.Log(DisplayName + ".AbilityEffect.ApplyInputMultiplier()");

            foreach (ResourceInputAmountNode resourceInputAmountNode in abilityEffectContext.resourceAmounts) {
                //Debug.Log(DisplayName + ".AbilityEffect.ApplyInputMultiplier(): before: " + resourceInputAmountNode.amount);
                resourceInputAmountNode.amount = (int)(resourceInputAmountNode.amount * inputMultiplier);
                //Debug.Log(DisplayName + ".AbilityEffect.ApplyInputMultiplier(): resource: " + resourceInputAmountNode.resourceName + "; after: " + resourceInputAmountNode.amount);
            }

            return abilityEffectContext;
        }

        public virtual void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {
            //public virtual void SetupScriptableObjects(SystemGameManager systemGameManager, string displayName) {
            //Debug.Log(DisplayName + ".AbilityEffect.SetupscriptableObjects()");

            //this.displayName = displayName;
            describableData = describable;

            Configure(systemGameManager);

            hitAbilityEffectList = new List<AbilityEffectProperties>();
            if (hitAbilityEffectNames != null) {
                foreach (string abilityEffectName in hitAbilityEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        hitAbilityEffectList.Add(abilityEffect.AbilityEffectProperties);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            onHitAudioProfiles = new List<AudioProfile>();
            if (onHitAudioProfileNames != null) {
                foreach (string audioProfileName in onHitAudioProfileNames) {
                    AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(audioProfileName);
                    if (audioProfile != null) {
                        onHitAudioProfiles.Add(audioProfile);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + audioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (effectMaterialName != null && effectMaterialName != string.Empty) {
                effectMaterial = null;
                MaterialProfile tmpMaterialProfile = systemDataFactory.GetResource<MaterialProfile>(effectMaterialName);
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