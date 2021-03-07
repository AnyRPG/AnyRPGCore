using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New StatusEffect", menuName = "AnyRPG/Abilities/Effects/StatusEffect")]
    public class StatusEffect : LengthEffect, ILearnable {

        [Header("Status Effect")]

        [SerializeField]
        private StatusEffectAlignment statusEffectAlignment = StatusEffectAlignment.None;

        [SerializeField]
        private string statusEffectTypeName = string.Empty;

        private StatusEffectType statusEffectType = null;

        [Header("Trait")]

        [Tooltip("Automatically cast on the character, active at all times, and do not appear on the status bar. Useful for class traits and equipment set bonuses.")]
        [SerializeField]
        protected bool classTrait = false;

        [Tooltip("The required level to automatically cast this if it is a trait")]
        [SerializeField]
        protected int requiredLevel = 1;

        [Header("Restrictions")]

        [Tooltip("Scenes this effect can be active in")]
        [SerializeField]
        protected List<string> sceneNames = new List<string>();

        [Header("Duration")]

        [Tooltip("by default all status effects are infinite duration")]
        [SerializeField]
        protected bool limitedDuration;

        [Tooltip("when an attempt to apply the effect is made, is the duration refreshed")]
        protected bool refreshableDuration = true;

        [Tooltip("If limited duration is true, the number of seconds this will be active for without haste or slow")]
        [SerializeField]
        protected float duration;

        [Header("Stack Size")]

        [Tooltip("the maximum number of stacks of this effect that can be applied at once")]
        [SerializeField]
        private int maxStacks = 1;

        [Header("Primary Stat Buffs and Debuffs")]

        [Tooltip("The values in this section will be applied to all of the following stats")]
        [SerializeField]
        protected List<string> statBuffTypeNames = new List<string>();

        [Tooltip("This amount will be added to the stats")]
        [SerializeField]
        protected int statAmount;

        [Tooltip("The stats will be multiplied by this amount (after addition)")]
        [SerializeField]
        protected float statMultiplier = 1f;

        [Header("Primary Stat Buffs and Debuffs")]

        [Tooltip("The values in this section will be applied to all of the following stats")]
        [SerializeField]
        protected List<SecondaryStatType> secondaryStatBuffsTypes = new List<SecondaryStatType>();

        [Tooltip("This amount will be added to the stats")]
        [SerializeField]
        protected int secondaryStatAmount;

        [Tooltip("The stats will be multiplied by this amount (after addition)")]
        [SerializeField]
        protected float secondaryStatMultiplier = 1;

        [Header("Damage Adjustments")]

        [Tooltip("Multiply outgoing damage by this amount.  1 = normal damage.")]
        [SerializeField]
        protected float outgoingDamageMultiplier = 1f;

        [Tooltip("Multiply incoming damage by this amount.  1 = normal damage.")]
        [SerializeField]
        protected float incomingDamageMultiplier = 1f;

        [Header("Faction Modifiers")]

        [Tooltip("Temporarily modify a character faction relationship while this buff is active")]
        [SerializeField]
        protected List<FactionDisposition> factionModifiers = new List<FactionDisposition>();

        [Header("Status Effects")]

        [Tooltip("Freeze the character and prevent all movement and animation.  Can be combined with different materials for statue, ice block, etc")]
        [SerializeField]
        protected bool disableAnimator = false;

        [Tooltip("Stun the character.  They cannot move, and a stun animation will be played.")]
        [SerializeField]
        protected bool stun = false;

        [Tooltip("Levitate the character.  They cannot move, and will hover above the ground.")]
        [SerializeField]
        protected bool levitate = false;

        [Header("Status Immunity")]

        [Tooltip("Immune to freeze effects")]
        [SerializeField]
        protected bool immuneDisableAnimator = false;

        [Tooltip("Immune to stun effects")]
        [SerializeField]
        protected bool immuneStun = false;

        [Tooltip("Immune to levitate effects")]
        [SerializeField]
        protected bool immuneLevitate = false;

        [Header("Target Control")]
        [Tooltip("If true, the target will mirror all actions taken by the caster and will not be able to control their actions")]
        [SerializeField]
        protected bool controlTarget = false;

        [Header("Reflect Effects")]

        [Tooltip("Ability Effects to cast when the character is hit with an attack")]
        [SerializeField]
        protected List<string> reflectAbilityEffectNames = new List<string>();

        protected List<AbilityEffect> reflectAbilityEffectList = new List<AbilityEffect>();

        [Header("Weapon Hit Effects")]

        [Tooltip("Ability Effects to cast when a weapon hit is scored on an enemy")]
        [SerializeField]
        protected List<string> weaponHitAbilityEffectNames = new List<string>();

        protected List<AbilityEffect> weaponHitAbilityEffectList = new List<AbilityEffect>();

        public int StatAmount { get => statAmount; }
        public float StatMultiplier { get => statMultiplier; set => statMultiplier = value; }
        public float IncomingDamageMultiplier { get => incomingDamageMultiplier; set => incomingDamageMultiplier = value; }
        public List<FactionDisposition> FactionModifiers { get => factionModifiers; set => factionModifiers = value; }
        public bool ControlTarget { get => controlTarget; set => controlTarget = value; }
        public bool DisableAnimator { get => disableAnimator; set => disableAnimator = value; }
        public bool Stun { get => stun; set => stun = value; }
        public bool Levitate { get => levitate; set => levitate = value; }
        public float Duration {
            get {
                if (limitedDuration == false) {
                    return 0f;
                }
                return duration;
            }
            set => duration = value;
        }
        public List<AbilityEffect> ReflectAbilityEffectList { get => reflectAbilityEffectList; set => reflectAbilityEffectList = value; }
        public List<AbilityEffect> WeaponHitAbilityEffectList { get => weaponHitAbilityEffectList; set => weaponHitAbilityEffectList = value; }
        public bool ClassTrait { get => classTrait; set => classTrait = value; }
        public bool LimitedDuration { get => limitedDuration; set => limitedDuration = value; }
        public int RequiredLevel { get => requiredLevel; set => requiredLevel = value; }
        public float OutgoingDamageMultiplier { get => outgoingDamageMultiplier; set => outgoingDamageMultiplier = value; }
        public bool ImmuneDisableAnimator { get => immuneDisableAnimator; set => immuneDisableAnimator = value; }
        public bool ImmuneStun { get => immuneStun; set => immuneStun = value; }
        public bool ImmuneLevitate { get => immuneLevitate; set => immuneLevitate = value; }
        public StatusEffectType StatusEffectType { get => statusEffectType; set => statusEffectType = value; }
        public StatusEffectAlignment StatusEffectAlignment { get => statusEffectAlignment; set => statusEffectAlignment = value; }
        public List<string> StatBuffTypeNames { get => statBuffTypeNames; set => statBuffTypeNames = value; }
        public List<SecondaryStatType> SecondaryStatBuffsTypes { get => secondaryStatBuffsTypes; set => secondaryStatBuffsTypes = value; }
        public int SecondaryStatAmount { get => secondaryStatAmount; set => secondaryStatAmount = value; }
        public float SecondaryStatMultiplier { get => secondaryStatMultiplier; set => secondaryStatMultiplier = value; }
        public List<string> SceneNames { get => sceneNames; set => sceneNames = value; }
        public bool RefreshableDuration { get => refreshableDuration; set => refreshableDuration = value; }
        public int MaxStacks { get => maxStacks; set => maxStacks = value; }

        public override void CancelEffect(BaseCharacter targetCharacter) {
            base.CancelEffect(targetCharacter);
            RemoveControlEffects(targetCharacter);
        }

        // bypass the creation of the status effect and just make its visual prefab
        public void RawCast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(MyName + ".StatusEffect.RawCast()");
            base.Cast(source, target, originalTarget, abilityEffectInput);
        }

        public override bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            if (classTrait == true && sourceCharacter.AbilityManager.Level >= requiredLevel) {
                return true;
            }
            if (!ZoneRequirementMet()) {
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". You are in the wrong zone");
                }
                return false;
            }
            return base.CanUseOn(target, sourceCharacter, abilityEffectContext, playerInitiated, performRangeCheck);
        }

        public bool ZoneRequirementMet() {
            if (SceneNames.Count > 0) {
                bool sceneFound = false;
                foreach (string sceneName in SceneNames) {
                    if (SystemResourceManager.prepareStringForMatch(sceneName) == SystemResourceManager.prepareStringForMatch(LevelManager.MyInstance.GetActiveSceneNode().SceneName)) {
                        sceneFound = true;
                    }
                }
                if (!sceneFound) {
                    return false;
                }
            }
            return true;
        }


        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".StatusEffect.Cast(" + source.AbilityManager.Name + ", " + (target? target.name : "null") + ")");
            if (abilityEffectContext.savedEffect == false && !CanUseOn(target, source)) {
                return null;
            }
            Dictionary<PrefabProfile, GameObject> returnObjects = null;
            CharacterStats targetCharacterStats = null;

            if ((classTrait || abilityEffectContext.savedEffect) && (source as BaseCharacter) is BaseCharacter) {
                targetCharacterStats = (source as BaseCharacter).CharacterStats;
            } else {
                if (target.CharacterUnit != null && target.CharacterUnit.BaseCharacter != null) {
                    targetCharacterStats = target.CharacterUnit.BaseCharacter.CharacterStats;
                }
            }

            // prevent status effect from sending scaled up damage to its ticks
            abilityEffectContext.castTimeMultiplier = 1f;

            StatusEffectNode _statusEffectNode = targetCharacterStats.ApplyStatusEffect(this, source, abilityEffectContext);
            if (_statusEffectNode == null) {
                //Debug.Log(DisplayName + ".StatusEffect.Cast(). statuseffect was null.  This could likely happen if the character already had the status effect max stack on them");
            } else {
                returnObjects = base.Cast(source, target, originalTarget, abilityEffectContext);
                if (returnObjects != null) {
                    // pass in the ability effect object so we can independently destroy it and let it last as long as the status effect (which could be refreshed).
                    _statusEffectNode.PrefabObjects = returnObjects;
                }
                PerformAbilityHit(source, target, abilityEffectContext);
            }
            return returnObjects;
        }

        public override void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log("DirectEffect.PerformAbilityEffect()");
            base.PerformAbilityHit(source, target, abilityEffectInput);
        }

        // THESE TWO EXIST IN DIRECTEFFECT ALSO BUT I COULD NOT FIND A GOOD WAY TO SHARE THEM
        public override void CastTick(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(abilityEffectName + ".StatusEffect.CastTick()");
            abilityEffectContext.spellDamageMultiplier = tickRate / Duration;
            base.CastTick(source, target, abilityEffectContext);
            PerformAbilityTick(source, target, abilityEffectContext);
        }

        public override void CastComplete(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".StatusEffect.CastComplete()");
            base.CastComplete(source, target, abilityEffectInput);
            PerformAbilityComplete(source, target, abilityEffectInput);
        }

        public virtual void CastWeaponHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.CastComplete(" + source.name + ", " + (target ? target.name : "null") + ")");
            PerformAbilityWeaponHit(source, target, abilityEffectInput);
        }

        public virtual void PerformAbilityWeaponHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.PerformAbilityTick(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityWeaponHitEffects(source, target, abilityEffectInput);
        }

        public virtual void PerformAbilityWeaponHitEffects(IAbilityCaster source, Interactable target, AbilityEffectContext effectOutput) {
            PerformAbilityEffects(source, target, effectOutput, weaponHitAbilityEffectList);
        }

        public virtual void CastReflect(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(MyName + ".AbilityEffect.CastReflect(" + source.Name + ", " + (target ? target.name : "null") + ")");
            PerformAbilityReflect(source, target, abilityEffectContext);
        }

        public virtual void PerformAbilityReflect(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityReflect(" + source.Name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityReflectEffects(source, target, abilityEffectContext);
        }


        public virtual void PerformAbilityReflectEffects(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".AbilityEffect.PerformAbilityReflectEffects(" + source.AbilityManager.UnitGameObject.name + ", " + (target == null ? "null" : target.gameObject.name) + ")");
            abilityEffectContext.reflectDamage = true;
            PerformAbilityEffects(source, target, abilityEffectContext, reflectAbilityEffectList);
        }

        public override string GetSummary() {
            //Debug.Log("StatusEffect.GetSummary()");
            string descriptionItem = string.Empty;
            string descriptionFinal = string.Empty;
            List<string> effectStrings = new List<string>();
            if (statBuffTypeNames.Count > 0) {

                foreach (string statBuffType in statBuffTypeNames) {
                    if (StatAmount > 0) {
                        descriptionItem = "Increases " + statBuffType + " by " + StatAmount;
                        effectStrings.Add(descriptionItem);
                    }
                    if (StatMultiplier > 0 && StatMultiplier < 1) {
                        descriptionItem = "Reduces " + statBuffType + " by " + ((1 - StatMultiplier) * 100) + "%";
                        effectStrings.Add(descriptionItem);
                    }
                    if (StatMultiplier > 1) {
                        descriptionItem = "Increases " + statBuffType + " by " + ((StatMultiplier - 1) * 100) + "%";
                        effectStrings.Add(descriptionItem);
                    }
                }
            }
            if (incomingDamageMultiplier > 1) {
                descriptionItem = "Multiplies all incoming damage by " + ((incomingDamageMultiplier - 1) * 100) + "%";
                effectStrings.Add(descriptionItem);
            } else if (incomingDamageMultiplier < 1) {
                descriptionItem = "Reduces all incoming damage by " + ((1 - incomingDamageMultiplier) * 100) + "%";
                effectStrings.Add(descriptionItem);
            }
            /*
            if (reflectAbilityEffectList != null) {
                description += "\nPerforms the following abilities "
                foreach (AbilityEffect abilityEffect in reflectAbilityEffectList) {

                }
            }
            */
            descriptionFinal = string.Empty;
            if (effectStrings.Count > 0) {
                descriptionFinal = "\n" + string.Join("\n", effectStrings);
            }
            string durationLabel = string.Empty;
            string statusText = string.Empty;
            float printedDuration;
            string durationString = string.Empty;

            if (limitedDuration == true && classTrait == false) {
                float remainingDuration = 0f;
                if (PlayerManager.MyInstance?.MyCharacter?.CharacterStats?.HasStatusEffect(this) == true) {
                    remainingDuration = PlayerManager.MyInstance.MyCharacter.CharacterStats.GetStatusEffectNode(this).RemainingDuration;
                }
                if (remainingDuration != 0f) {
                    durationLabel = "Remaining Duration: ";
                    printedDuration = (int)remainingDuration;
                } else {
                    durationLabel = "Duration: ";
                    printedDuration = (int)Duration;
                }
                statusText = SystemAbilityController.GetTimeText(printedDuration);
                if (durationLabel != string.Empty) {
                    durationString = "\n" + durationLabel + statusText;
                }
            }
            return base.GetSummary() + string.Format("{0}{1}", descriptionFinal, durationString);
        }

        public void ApplyControlEffects(BaseCharacter targetCharacter) {
            //Debug.Log(DisplayName + ".StatusEffect.ApplyControlEffects(" + (targetCharacter == null ? "null" : targetCharacter.CharacterName) + ")");
            if (targetCharacter == null) {
                //Debug.Log(DisplayName + ".StatusEffect.ApplyControlEffects() targetCharacter is null");
                return;
            }

            if (DisableAnimator == true) {
                //Debug.Log(abilityEffectName + ".StatusEffect.Tick() disabling animator and motor (freezing)");
                targetCharacter.UnitController.FreezeCharacter();
            }

            if (Stun == true) {
                targetCharacter.UnitController.StunCharacter();
            }
            if (Levitate == true) {
                //Debug.Log(abilityEffectName + ".StatusEffect.Tick() levitating");
                targetCharacter.UnitController.LevitateCharacter();
            }
        }

        public void RemoveControlEffects(BaseCharacter targetCharacter) {
            if (targetCharacter == null) {
                return;
            }
            if (DisableAnimator == true) {
                targetCharacter.UnitController.UnFreezeCharacter();
            }
            if (Stun == true) {
                targetCharacter.UnitController.UnStunCharacter();
            }
            if (Levitate == true) {
                targetCharacter.UnitController.UnLevitateCharacter();
            }
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            reflectAbilityEffectList = new List<AbilityEffect>();
            if (reflectAbilityEffectNames != null) {
                foreach (string abilityEffectName in reflectAbilityEffectNames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffectName);
                    if (abilityEffect != null) {
                        reflectAbilityEffectList.Add(abilityEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            weaponHitAbilityEffectList = new List<AbilityEffect>();
            if (weaponHitAbilityEffectNames != null) {
                foreach (string abilityEffectName in weaponHitAbilityEffectNames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffectName);
                    if (abilityEffect != null) {
                        weaponHitAbilityEffectList.Add(abilityEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (statusEffectTypeName != null && statusEffectTypeName != string.Empty) {
                StatusEffectType tmpStatusEffectType = SystemStatusEffectTypeManager.MyInstance.GetResource(statusEffectTypeName);
                if (tmpStatusEffectType != null) {
                    statusEffectType = tmpStatusEffectType;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find status effect type: " + statusEffectTypeName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }


            if (factionModifiers != null) {
                foreach (FactionDisposition factionDisposition in factionModifiers) {
                    if (factionDisposition != null) {
                        factionDisposition.SetupScriptableObjects();
                    }
                }
            }


        }

    }

    public enum SecondaryStatType { MovementSpeed, Accuracy, CriticalStrike, Speed, Damage, PhysicalDamage, SpellDamage, Armor }

    public enum StatusEffectAlignment { None, Beneficial, Harmful }
}
