using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New StatusEffect", menuName = "Abilities/Effects/StatusEffect")]
public class StatusEffect : LengthEffect {

    // the number of seconds this will be active for without haste or slow
    [SerializeField]
    protected float duration;

    // the maximum number of stacks of this effect that can be applied at once
    public int maxStacks = 1;

    // when an attempt to apply the effect is made, is the duration refreshed
    public bool refreshableDuration = true;

    private int currentStacks = 1;

    [SerializeField]
    protected int statAmount;

    [SerializeField]
    protected float statMultiplier = 1;

    [SerializeField]
    protected float incomingDamageMultiplier = 1f;

    [SerializeField]
    protected List<StatBuffType> statBuffTypes = new List<StatBuffType>();

    [SerializeField]
    protected List<FactionDisposition> factionModifiers = new List<FactionDisposition>();

    [SerializeField]
    protected bool disableAnimator = false;

    [SerializeField]
    protected bool stun = false;

    [SerializeField]
    protected bool levitate = false;

    // if true, the target will mirror all actions taken by the caster
    [SerializeField]
    protected bool controlTarget = false;

    protected float remainingDuration;

    // list of status effect nodes to send updates to so multiple effects panels and bars can access this
    private List<StatusEffectNodeScript> statusEffectNodeScripts = new List<StatusEffectNodeScript>();

    // any abilities to cast every tick
    [SerializeField]
    protected List<AbilityEffect> reflectAbilityEffectList = new List<AbilityEffect>();

    public int MyStatAmount { get => statAmount; }
    public List<StatBuffType> MyStatBuffTypes { get => statBuffTypes; set => statBuffTypes = value; }
    public float MyStatMultiplier { get => statMultiplier; set => statMultiplier = value; }
    public int MyCurrentStacks { get => currentStacks; set => currentStacks = value; }
    public float MyIncomingDamageMultiplier { get => incomingDamageMultiplier; set => incomingDamageMultiplier = value; }
    public List<FactionDisposition> MyFactionModifiers { get => factionModifiers; set => factionModifiers = value; }
    public bool MyControlTarget { get => controlTarget; set => controlTarget = value; }
    public bool MyDisableAnimator { get => disableAnimator; set => disableAnimator = value; }
    public bool MyStun { get => stun; set => stun = value; }
    public bool MyLevitate { get => levitate; set => levitate = value; }
    public float MyDuration { get => duration; set => duration = value; }

    public override void CancelEffect() {
        base.CancelEffect();
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

                // set updated values
                if (statusEffectNodeScript.MyUseTimerText == true) {
                    if (statusEffectNodeScript.MyTimer != null) {
                        if (statusEffectNodeScript.MyTimer.isActiveAndEnabled == false) {
                            statusEffectNodeScript.MyTimer.gameObject.SetActive(true);
                        }
                        statusEffectNodeScript.MyTimer.text = statusText;
                    }
                } else {
                    if (statusEffectNodeScript.MyTimer != null) {
                        statusEffectNodeScript.MyTimer.gameObject.SetActive(false); ;
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
    public void RawCast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
        base.Cast(source, target, originalTarget, abilityEffectInput);
    }

    public override void Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target? target.name : "null") + ")");
        if (!CanUseOn(target, source)) {
            return;
        }
        StatusEffectNode _statusEffectNode = target.GetComponent<CharacterUnit>().MyCharacter.MyCharacterStats.ApplyStatusEffect(SystemAbilityEffectManager.MyInstance.GetResource(MyName) as StatusEffect, source, target.GetComponent<CharacterUnit>(), abilityEffectInput);
        if (_statusEffectNode == null) {
            //Debug.Log("StatusEffect.Cast(). statuseffect was null.  This could likely happen if the character already had the status effect max stack on them");
        } else {
            base.Cast(source, target, originalTarget, abilityEffectInput);
            if (abilityEffectObject != null) {
                // pass in the ability effect object so we can independently destroy it and let it last as long as the status effect (which could be refreshed).
                _statusEffectNode.MyStatusEffect.abilityEffectObject = abilityEffectObject;
            }
            PerformAbilityHit(source, target, abilityEffectInput);
        }
    }

    public override void PerformAbilityHit(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
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

    public override void Initialize(BaseCharacter source, BaseCharacter target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(MyAbilityEffectName + ".StatusEffect.Initialize(" + source.name + ", " + target.name + ")");
        base.Initialize(source, target, abilityEffectInput);
        //co = (target.MyCharacterStats as CharacterStats).StartCoroutine(Tick(source, abilityEffectInput, target));
        //co = (target.MyCharacterStats as CharacterStats).StartCoroutine(Tick(source, abilityEffectInput, target, this));
    }

    //public void HandleStatusEffectEnd(

    // THESE TWO EXIST IN DIRECTEFFECT ALSO BUT I COULD NOT FIND A GOOD WAY TO SHARE THEM
    public override void CastTick(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(abilityEffectName + ".StatusEffect.CastTick()");
        base.CastTick(source, target, abilityEffectInput);
        PerformAbilityTick(source, target, abilityEffectInput);
    }

    public override void CastComplete(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(abilityEffectName + ".StatusEffect.CastComplete()");
        base.CastComplete(source, target, abilityEffectInput);
        PerformAbilityComplete(source, target, abilityEffectInput);
    }

    public virtual void CastReflect(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(abilityEffectName + ".AbilityEffect.CastComplete(" + source.name + ", " + (target ? target.name : "null") + ")");
        PerformAbilityReflect(source, target, abilityEffectInput);
    }

    public virtual void PerformAbilityReflect(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(abilityEffectName + ".AbilityEffect.PerformAbilityTick(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
        PerformAbilityReflectEffects(source, target, abilityEffectInput);
    }


    public virtual void PerformAbilityReflectEffects(BaseCharacter source, GameObject target, AbilityEffectOutput effectOutput) {
        PerformAbilityEffects(source, target, effectOutput, reflectAbilityEffectList);
    }

    public override string GetSummary() {
        //Debug.Log("StatusEffect.GetSummary()");
        string descriptionItem = string.Empty;
        string descriptionFinal = string.Empty;
        List<string> effectStrings = new List<string>();
        if (MyStatBuffTypes.Count > 0) {

            foreach (StatBuffType statBuffType in statBuffTypes) {
                if (MyStatAmount > 0) {
                    descriptionItem = "Increases " + statBuffType.ToString() + " by " + MyStatAmount;
                    effectStrings.Add(descriptionItem);
                }
                if (MyStatMultiplier > 0 && MyStatMultiplier < 1) {
                    descriptionItem = "Reduces " + statBuffType.ToString() + " by " + ((1 - MyStatMultiplier) * 100) + "%";
                    effectStrings.Add(descriptionItem);
                }
                if (MyStatMultiplier > 1) {
                    descriptionItem = "Increases " + statBuffType.ToString() + " by " + ((MyStatMultiplier - 1) * 100) + "%";
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
        descriptionFinal = string.Join("\n", effectStrings);
        string durationLabel = "Duration: ";
        float printedDuration;

        if (remainingDuration != 0f) {
            durationLabel = "Remaining Duration: ";
            printedDuration = (int)remainingDuration;
        } else {
            printedDuration = (int)duration;
        }
        string statusText = string.Empty;
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
        return string.Format("{0}\n{1}{2}", descriptionFinal, durationLabel, statusText);
    }


}

public enum StatBuffType { Stamina, Strength, Intellect, Agility, MovementSpeed }