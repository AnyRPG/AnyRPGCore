using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Gather Ability",menuName = "AnyRPG/Abilities/Effects/GatherAbility")]
    public class GatherAbility : DirectAbility {

        public override bool Cast(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            if (target == null) {
                return false;
            }
            bool returnResult = base.Cast(source, target, abilityEffectContext);
            if (returnResult == true) {
                if (target != null) {
                    GatheringNodeComponent gatheringNodeComponent = GatheringNodeComponent.GetGatheringNodeComponent(target);
                    if (gatheringNodeComponent != null) {
                        gatheringNodeComponent.Gather();
                    }
                } else {
                    //Debug.Log(MyName + ".GatherAbility.Cast(): target was null");
                }
            }
            return returnResult;
        }

        public override bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false) {
            //Debug.Log(MyName + ".GatherAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + (sourceCharacter == null ? "null" : sourceCharacter.AbilityManager.MyName) + ")");
            if (target != null) {
                //Debug.Log("GatherAbility.CanUseOn(" + target.name + ")");
            } else {
                //Debug.Log("GatherAbility.CanUseOn(null)");
            }
            if (!base.CanUseOn(target, sourceCharacter, performCooldownChecks, abilityEffectContext, playerInitiated)) {
                return false;
            }
            // distance from center of character to whereever the raycast hit the object
            //float distanceToTarget = Vector3.Distance((PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).MyMouseOverhit.point, source.AbilityManager.UnitGameObject.transform.TransformPoint(source.MyCharacterUnit.GetComponent<CapsuleCollider>().center));
            //Debug.Log("PlayerManager.MyInstance.MyCharacter.MyCharacterController.MyMouseOverhit.point: " + PlayerManager.MyInstance.MyCharacter.MyCharacterController.MyMouseOverhit.point);

            /*
            if (distanceToTarget > (source.MyCharacterStats.MyHitBox * 2)) {
                //Debug.Log(target.name + " is out of range: " + distanceToTarget);
                MessageFeedManager.MyInstance.WriteMessage("Gathering node was out of range");
                return false;
            }
            */

            GatheringNodeComponent gatheringNodeComponent = GatheringNodeComponent.GetGatheringNodeComponent(target);
            if (gatheringNodeComponent == null) {
                //Debug.Log("You cannot use " + MyName + " on: " + target.name);
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". This ability must target a gathering node");
                }
                return false;
            }

            if (gatheringNodeComponent.BaseAbility == this) {
                return true;
            } else {
                //Debug.Log(target.name + " requires ability: " + _gatheringNode.MyAbility);
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". This gathering node requires the skill : " + gatheringNodeComponent.BaseAbility.DisplayName);
                }
                return false;
            }
        }

    }

}