using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewAnimatedAbility", menuName = "Abilities/AnimatedAbility")]
public class AnimatedAbility : BaseAbility {

    public override void Cast(BaseCharacter source, GameObject target, Vector3 groundTarget) {
        //Debug.Log("AnimatedAbility.Cast()");
        base.Cast(source, target, groundTarget);
        if (animationClip != null) {
            //Debug.Log("AnimatedAbility.Cast(): animationClip is not null, setting animator");

            // this type of ability is allowed to interrupt other types of animations, so clear them all
            source.MyCharacterUnit.MyCharacterAnimator.ClearAnimationBlockers();

            // now block further animations of other types from starting
            source.MyCharacterAbilityManager.MyWaitingForAnimatedAbility = true;

            // perform the actual animation
            source.MyCharacterUnit.MyCharacterAnimator.HandleAbility(animationClip, this);

            // unblock 
            source.MyCharacterUnit.MyCharacter.MyCharacterCombat.OnHitEvent += HandleAbilityHit;
        }
    }

    public void CleanupEventReferences(BaseCharacter source) {
        source.MyCharacterCombat.OnHitEvent -= HandleAbilityHit;
    }

    public void HandleAbilityHit(BaseCharacter source, GameObject target) {
        //Debug.Log("AnimatedAbility.HandleAbilityHit(): setting waiting for animated ability to false");
        PerformAbilityEffects(source, target, Vector3.zero);
    }

    public override string GetSummary() {
        string returnString = base.GetSummary();
        return returnString;

    }

    public override bool CanUseOn(GameObject target, BaseCharacter source) {
        //Debug.Log("AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + source.MyCharacterName + ")");
        if (source.MyCharacterAbilityManager.MyWaitingForAnimatedAbility == true) {
            //Debug.Log("AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + source.MyCharacterName + ") FAILING.  ALREADY IN PROGRESS");
            CombatLogUI.MyInstance.WriteCombatMessage("Cannot use " + MyName + ". Waiting for another ability to finish.");
            return false;
        }
        //Debug.Log("AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + source.MyCharacterName + ") returning base");
        return base.CanUseOn(target, source);
    }

}

public enum AnyRPGWeaponAffinity { Unarmed, Sword2H, Sword1H, Staff, Shield, Mace2H, Mace1H }