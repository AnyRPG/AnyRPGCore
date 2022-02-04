using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class StatusEffectProperties : LengthEffectProperties {

        [Header("Status Effect")]

        [SerializeField]
        private StatusEffectAlignment statusEffectAlignment = StatusEffectAlignment.None;

        [Tooltip("Set this value to determine the status effect type for the purpose of removeEffects (eg remove bleed or remove poison)")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(StatusEffectType))]
        private string statusEffectTypeName = string.Empty;

        private StatusEffectType statusEffectType = null;

        [Tooltip("Only one status effect with this group name can be on a character at a time")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(StatusEffectGroup))]
        private string statusEffectGroupName = string.Empty;

        protected StatusEffectGroup statusEffectGroup = null;

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

        [Tooltip("If true, the character can fly")]
        [SerializeField]
        protected bool canFly = false;

        [Tooltip("If true, the character can glide")]
        [SerializeField]
        protected bool canGlide = false;

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
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        protected List<string> reflectAbilityEffectNames = new List<string>();

        protected List<AbilityEffect> reflectAbilityEffectList = new List<AbilityEffect>();

        [Header("Weapon Hit Effects")]

        [Tooltip("Ability Effects to cast when a weapon hit is scored on an enemy")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        protected List<string> weaponHitAbilityEffectNames = new List<string>();

        protected List<AbilityEffect> weaponHitAbilityEffectList = new List<AbilityEffect>();

        // game manager references
        protected LevelManager levelManager = null;
        protected PlayerManager playerManager = null;

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
        public bool CanFly { get => canFly; }
        public bool CanGlide { get => canGlide; }
        public StatusEffectGroup StatusEffectGroup { get => statusEffectGroup; set => statusEffectGroup = value; }

       

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, string displayName) {
            base.SetupScriptableObjects(systemGameManager, displayName);

            reflectAbilityEffectList = new List<AbilityEffect>();
            if (reflectAbilityEffectNames != null) {
                foreach (string abilityEffectName in reflectAbilityEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
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
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        weaponHitAbilityEffectList.Add(abilityEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (statusEffectTypeName != null && statusEffectTypeName != string.Empty) {
                StatusEffectType tmpStatusEffectType = systemDataFactory.GetResource<StatusEffectType>(statusEffectTypeName);
                if (tmpStatusEffectType != null) {
                    statusEffectType = tmpStatusEffectType;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find status effect type: " + statusEffectTypeName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (statusEffectGroupName != null && statusEffectGroupName != string.Empty) {
                StatusEffectGroup tmpStatusEffectGroup = systemDataFactory.GetResource<StatusEffectGroup>(statusEffectGroupName);
                if (tmpStatusEffectGroup != null) {
                    statusEffectGroup = tmpStatusEffectGroup;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find status effect group: " + statusEffectGroupName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }


            if (factionModifiers != null) {
                foreach (FactionDisposition factionDisposition in factionModifiers) {
                    if (factionDisposition != null) {
                        factionDisposition.SetupScriptableObjects(systemDataFactory);
                    }
                }
            }


        }

    }

    public enum SecondaryStatType { MovementSpeed, Accuracy, CriticalStrike, Speed, Damage, PhysicalDamage, SpellDamage, Armor }

    public enum StatusEffectAlignment { None, Beneficial, Harmful }

}
