using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New StatusEffect", menuName = "AnyRPG/Abilities/Effects/StatusEffect")]
    public class StatusEffect : LengthEffect {

        [SerializeField]
        private StatusEffectAlignment statusEffectAlignment = StatusEffectAlignment.None;

        [SerializeField]
        private string statusEffectTypeName = string.Empty;

        private StatusEffectType statusEffectType = null;

        [Header("Trait")]

        [Tooltip("Automatically cast on the character, active at all times, and do not appear on the status bar. Useful for class traits and equipment set bonuses.")]
        [SerializeField]
        protected bool classTrait;

        [Tooltip("The required level to automatically cast this if it is a trait")]
        [SerializeField]
        private int requiredLevel = 1;

        [Header("Duration")]

        [Tooltip("by default all status effects are infinite duration")]
        [SerializeField]
        protected bool limitedDuration;

        [Tooltip("when an attempt to apply the effect is made, is the duration refreshed")]
        public bool refreshableDuration = true;

        [Tooltip("If limited duration is true, the number of seconds this will be active for without haste or slow")]
        [SerializeField]
        protected float duration;

        [Header("Stack Size")]

        [Tooltip("the maximum number of stacks of this effect that can be applied at once")]
        public int maxStacks = 1;

        private int currentStacks = 1;

        [Header("Primary Stat Buffs and Debuffs")]

        [Tooltip("The values in this section will be applied to all of the following stats")]
        [SerializeField]
        protected List<string> statBuffTypeNames = new List<string>();

        [Tooltip("This amount will be added to the stats")]
        [SerializeField]
        protected int statAmount;

        [Tooltip("The stats will be multiplied by this amount (after addition)")]
        [SerializeField]
        protected float statMultiplier = 1;

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

        protected float remainingDuration;

        // list of status effect nodes to send updates to so multiple effects panels and bars can access this
        private List<StatusEffectNodeScript> statusEffectNodeScripts = new List<StatusEffectNodeScript>();

        [Tooltip("Additional Effects")]

        [SerializeField]
        protected List<string> reflectAbilityEffectNames = new List<string>();

        [SerializeField]
        protected List<AbilityEffect> reflectAbilityEffectList = new List<AbilityEffect>();

        [SerializeField]
        protected List<string> weaponHitAbilityEffectNames = new List<string>();

        [SerializeField]
        protected List<AbilityEffect> weaponHitAbilityEffectList = new List<AbilityEffect>();

        public int MyStatAmount { get => statAmount; }
        public float MyStatMultiplier { get => statMultiplier; set => statMultiplier = value; }
        public int MyCurrentStacks { get => currentStacks; set => currentStacks = value; }
        public float IncomingDamageMultiplier { get => incomingDamageMultiplier; set => incomingDamageMultiplier = value; }
        public List<FactionDisposition> MyFactionModifiers { get => factionModifiers; set => factionModifiers = value; }
        public bool MyControlTarget { get => controlTarget; set => controlTarget = value; }
        public bool MyDisableAnimator { get => disableAnimator; set => disableAnimator = value; }
        public bool MyStun { get => stun; set => stun = value; }
        public bool MyLevitate { get => levitate; set => levitate = value; }
        public float MyDuration { get => duration; set => duration = value; }
        public List<AbilityEffect> MyReflectAbilityEffectList { get => reflectAbilityEffectList; set => reflectAbilityEffectList = value; }
        public List<AbilityEffect> MyWeaponHitAbilityEffectList { get => weaponHitAbilityEffectList; set => weaponHitAbilityEffectList = value; }
        public bool MyClassTrait { get => classTrait; set => classTrait = value; }
        public bool MyLimitedDuration { get => limitedDuration; set => limitedDuration = value; }
        public int MyRequiredLevel { get => requiredLevel; set => requiredLevel = value; }
        public float MyOutgoingDamageMultiplier { get => outgoingDamageMultiplier; set => outgoingDamageMultiplier = value; }
        public bool MyImmuneDisableAnimator { get => immuneDisableAnimator; set => immuneDisableAnimator = value; }
        public bool MyImmuneStun { get => immuneStun; set => immuneStun = value; }
        public bool MyImmuneLevitate { get => immuneLevitate; set => immuneLevitate = value; }
        public StatusEffectType MyStatusEffectType { get => statusEffectType; set => statusEffectType = value; }
        public StatusEffectAlignment MyStatusEffectAlignment { get => statusEffectAlignment; set => statusEffectAlignment = value; }
        public List<string> StatBuffTypeNames { get => statBuffTypeNames; set => statBuffTypeNames = value; }
        public List<SecondaryStatType> SecondaryStatBuffsTypes { get => secondaryStatBuffsTypes; set => secondaryStatBuffsTypes = value; }
        public int SecondaryStatAmount { get => secondaryStatAmount; set => secondaryStatAmount = value; }
        public float SecondaryStatMultiplier { get => secondaryStatMultiplier; set => secondaryStatMultiplier = value; }

        public override void CancelEffect(BaseCharacter targetCharacter) {
            base.CancelEffect(targetCharacter);
            RemoveControlEffects(targetCharacter);
            ClearNodeScripts();
        }

        public void ClearNodeScripts() {
            foreach (StatusEffectNodeScript statusEffectNodeScript in statusEffectNodeScripts) {
                if (statusEffectNodeScript != null) {
                    //Debug.Log("AbilityEffect.OnDestroy() statusEffectNodeScript is not null. destroying gameobject");
                    Destroy(statusEffectNodeScript.gameObject);
                } else {
                    //Debug.Log("AbilityEffect.OnDestroy() statusEffectNodeScript is null!");
                }
            }
            statusEffectNodeScripts.Clear();
        }

        public void SetStatusNode(StatusEffectNodeScript statusEffectNodeScript) {
            //Debug.Log("StatusEffect.SetStatusNode()");
            statusEffectNodeScripts.Add(statusEffectNodeScript);
            UpdateStatusNode();
        }

        public void UpdateStatusNode() {
            //Debug.Log(GetInstanceID() + ".StatusEffect.UpdateStatusNode(): COUNT statusEffectNodeScript: " + statusEffectNodeScripts.Count);
            foreach (StatusEffectNodeScript statusEffectNodeScript in statusEffectNodeScripts) {
                //Debug.Log("StatusEffect.UpdateStatusNode(): got statuseffectnodescript");
                if (statusEffectNodeScript != null) {
                    string statusText = string.Empty;
                    string stackText = string.Empty;
                    if (currentStacks > 1) {
                        stackText = currentStacks.ToString();
                    }
                    if (limitedDuration == true && classTrait == false) {
                        //Debug.Log(GetInstanceID() + MyName + ".StatusEffect.UpdateStatusNode(): limted");
                        float printedDuration = (int)remainingDuration;
                        if (printedDuration < 60 && printedDuration >= 0) {
                            // less than 1 minute
                            statusText = ((int)printedDuration).ToString() + "s";
                        } else if (printedDuration < 3600) {
                            //less than 1 hour
                            statusText = ((int)(printedDuration / 60)).ToString() + "m";
                        } else if (printedDuration > 3600f) {
                            //greater than 1 hour
                            statusText = ((int)(printedDuration / 3600)).ToString() + "h";
                        }
                    }

                    // set updated values
                    if (statusEffectNodeScript.MyUseTimerText == true && statusText != string.Empty) {
                        if (statusEffectNodeScript.MyTimer != null) {
                            if (statusEffectNodeScript.MyTimer.isActiveAndEnabled == false) {
                                statusEffectNodeScript.MyTimer.gameObject.SetActive(true);
                            }
                            statusEffectNodeScript.MyTimer.text = statusText;
                        }
                    } else {
                        if (statusEffectNodeScript.MyTimer != null) {
                            statusEffectNodeScript.MyTimer.gameObject.SetActive(false);
                        }
                    }
                    if (statusEffectNodeScript.MyUseStackText == true) {
                        if (statusEffectNodeScript.MyStackCount.isActiveAndEnabled == false) {
                            statusEffectNodeScript.MyStackCount.gameObject.SetActive(true);
                        }
                        statusEffectNodeScript.MyStackCount.gameObject.SetActive(true);
                        statusEffectNodeScript.MyStackCount.text = stackText;
                    } else {
                        if (statusEffectNodeScript.MyStackCount != null) {
                            statusEffectNodeScript.MyStackCount.gameObject.SetActive(false);
                        }
                    }
                    float usedFillAmount = remainingDuration / duration;
                    statusEffectNodeScript.UpdateFillIcon((usedFillAmount * -1f) + 1f);
                }
            }
        }

        // bypass the creation of the status effect and just make its visual prefab
        public void RawCast(IAbilityCaster source, GameObject target, GameObject originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(MyName + ".StatusEffect.RawCast()");
            base.Cast(source, target, originalTarget, abilityEffectInput);
        }

        public override bool CanUseOn(GameObject target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null) {
            if (classTrait == true && sourceCharacter.Level >= requiredLevel) {
                return true;
            }
            return base.CanUseOn(target, sourceCharacter, abilityEffectContext);
        }


        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, GameObject target, GameObject originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target? target.name : "null") + ")");
            if (!abilityEffectInput.savedEffect && !CanUseOn(target, source)) {
                return null;
            }
            Dictionary<PrefabProfile, GameObject> returnObjects = null;
            CharacterStats targetCharacterStats = null;

            if ((classTrait || abilityEffectInput.savedEffect) && (source as CharacterAbilityManager) is CharacterAbilityManager) {
                targetCharacterStats = (source as CharacterAbilityManager).BaseCharacter.CharacterStats;
            } else {
                targetCharacterStats = target.GetComponent<CharacterUnit>().MyCharacter.CharacterStats;
            }

            // prevent status effect from sending scaled up damage to its ticks
            abilityEffectInput.castTimeMultiplier = 1f;

            StatusEffectNode _statusEffectNode = targetCharacterStats.ApplyStatusEffect(SystemAbilityEffectManager.MyInstance.GetNewResource(MyName) as StatusEffect, source, abilityEffectInput);
            if (_statusEffectNode == null) {
                //Debug.Log("StatusEffect.Cast(). statuseffect was null.  This could likely happen if the character already had the status effect max stack on them");
            } else {
                returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
                if (returnObjects != null) {
                    // pass in the ability effect object so we can independently destroy it and let it last as long as the status effect (which could be refreshed).
                    _statusEffectNode.MyStatusEffect.MyPrefabObjects = returnObjects;
                }
                PerformAbilityHit(source, target, abilityEffectInput);
            }
            return returnObjects;
        }

        public override void PerformAbilityHit(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log("DirectEffect.PerformAbilityEffect()");
            base.PerformAbilityHit(source, target, abilityEffectInput);
        }


        public bool AddStack() {
            bool returnValue = false;
            if (currentStacks < maxStacks) {
                currentStacks++;
                // refresh the duration
                returnValue = true;
            }
            if (refreshableDuration) {
                SetRemainingDuration(duration);
            }
            return returnValue;
        }

        public void SetRemainingDuration(float remainingDuration) {
            this.remainingDuration = remainingDuration;
        }

        public float GetRemainingDuration() {
            return remainingDuration;
        }

        public override void Initialize(IAbilityCaster source, BaseCharacter target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(MyAbilityEffectName + ".StatusEffect.Initialize(" + source.name + ", " + target.name + ")");
            base.Initialize(source, target, abilityEffectInput);
            //co = (target.MyCharacterStats as CharacterStats).StartCoroutine(Tick(source, abilityEffectInput, target));
            //co = (target.MyCharacterStats as CharacterStats).StartCoroutine(Tick(source, abilityEffectInput, target, this));
        }

        //public void HandleStatusEffectEnd(

        // THESE TWO EXIST IN DIRECTEFFECT ALSO BUT I COULD NOT FIND A GOOD WAY TO SHARE THEM
        public override void CastTick(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".StatusEffect.CastTick()");
            abilityEffectInput.spellDamageMultiplier = tickRate / duration;
            base.CastTick(source, target, abilityEffectInput);
            PerformAbilityTick(source, target, abilityEffectInput);
        }

        public override void CastComplete(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".StatusEffect.CastComplete()");
            base.CastComplete(source, target, abilityEffectInput);
            PerformAbilityComplete(source, target, abilityEffectInput);
        }

        public virtual void CastWeaponHit(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.CastComplete(" + source.name + ", " + (target ? target.name : "null") + ")");
            PerformAbilityWeaponHit(source, target, abilityEffectInput);
        }

        public virtual void PerformAbilityWeaponHit(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.PerformAbilityTick(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityWeaponHitEffects(source, target, abilityEffectInput);
        }


        public virtual void PerformAbilityWeaponHitEffects(IAbilityCaster source, GameObject target, AbilityEffectContext effectOutput) {
            PerformAbilityEffects(source, target, effectOutput, weaponHitAbilityEffectList);
        }

        public virtual void CastReflect(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(MyName + ".AbilityEffect.CastReflect(" + source.name + ", " + (target ? target.name : "null") + ")");
            PerformAbilityReflect(source, target, abilityEffectInput);
        }

        public virtual void PerformAbilityReflect(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.PerformAbilityTick(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityReflectEffects(source, target, abilityEffectInput);
        }


        public virtual void PerformAbilityReflectEffects(IAbilityCaster source, GameObject target, AbilityEffectContext effectOutput) {
            effectOutput.refectDamage = true;
            PerformAbilityEffects(source, target, effectOutput, reflectAbilityEffectList);
        }

        public override string GetSummary() {
            //Debug.Log("StatusEffect.GetSummary()");
            string descriptionItem = string.Empty;
            string descriptionFinal = string.Empty;
            List<string> effectStrings = new List<string>();
            if (statBuffTypeNames.Count > 0) {

                foreach (string statBuffType in statBuffTypeNames) {
                    if (MyStatAmount > 0) {
                        descriptionItem = "Increases " + statBuffType + " by " + MyStatAmount;
                        effectStrings.Add(descriptionItem);
                    }
                    if (MyStatMultiplier > 0 && MyStatMultiplier < 1) {
                        descriptionItem = "Reduces " + statBuffType + " by " + ((1 - MyStatMultiplier) * 100) + "%";
                        effectStrings.Add(descriptionItem);
                    }
                    if (MyStatMultiplier > 1) {
                        descriptionItem = "Increases " + statBuffType + " by " + ((MyStatMultiplier - 1) * 100) + "%";
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
                if (remainingDuration != 0f) {
                    durationLabel = "Remaining Duration: ";
                    printedDuration = (int)remainingDuration;
                } else {
                    durationLabel = "Duration: ";
                    printedDuration = (int)duration;
                }
                if (printedDuration < 60 && printedDuration >= 0) {
                    // less than 1 minute
                    statusText = ((int)printedDuration).ToString() + " second";
                    if ((int)printedDuration != 1) {
                        statusText += "s";
                    }
                } else if (printedDuration < 3600) {
                    //less than 1 hour
                    statusText = ((int)(printedDuration / 60)).ToString() + " minute";
                    if (((int)printedDuration / 60) != 1) {
                        statusText += "s";
                    }
                } else if (printedDuration > 3600f) {
                    //greater than 1 hour
                    statusText = ((int)(printedDuration / 3600)).ToString() + " hour";
                    if (((int)printedDuration / 3600) != 1) {
                        statusText += "s";
                    }
                }
                if (durationLabel != string.Empty) {
                    durationString = "\n" + durationLabel + statusText;
                }
            }
            return base.GetSummary() + string.Format("{0}{1}", descriptionFinal, durationString);
        }

        public void ApplyControlEffects(BaseCharacter targetCharacter) {
            if (MyDisableAnimator == true) {
                //Debug.Log(abilityEffectName + ".StatusEffect.Tick() disabling animator and motor (freezing)");
                targetCharacter.CharacterController.FreezeCharacter();
            }

            if (MyStun == true) {
                //Debug.Log(abilityEffectName + ".StatusEffect.Tick() stunning");
                targetCharacter.CharacterController.StunCharacter();
            }
            if (MyLevitate == true) {
                //Debug.Log(abilityEffectName + ".StatusEffect.Tick() levitating");
                targetCharacter.CharacterController.LevitateCharacter();
            }
        }

        public void RemoveControlEffects(BaseCharacter targetCharacter) {
            if (targetCharacter == null) {
                return;
            }
            if (MyDisableAnimator == true) {
                targetCharacter.CharacterController.UnFreezeCharacter();
            }
            if (MyStun == true) {
                targetCharacter.CharacterController.UnStunCharacter();
            }
            if (MyLevitate == true) {
                targetCharacter.CharacterController.UnLevitateCharacter();
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
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (statusEffectTypeName != null && statusEffectTypeName != string.Empty) {
                StatusEffectType tmpStatusEffectType = SystemStatusEffectTypeManager.MyInstance.GetResource(statusEffectTypeName);
                if (tmpStatusEffectType != null) {
                    statusEffectType = tmpStatusEffectType;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find status effect type: " + statusEffectTypeName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
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
