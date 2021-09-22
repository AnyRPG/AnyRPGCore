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
                    //Debug.Log(DisplayName + ".GatherAbility.Cast(): target was null");
                }
            }
            return returnResult;
        }

        public override bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            //Debug.Log(DisplayName + ".GatherAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + (sourceCharacter == null ? "null" : sourceCharacter.AbilityManager.DisplayName) + ")");
            if (!base.CanUseOn(target, sourceCharacter, performCooldownChecks, abilityEffectContext, playerInitiated, performRangeCheck)) {
                return false;
            }

            if (target == null) {
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". Gathering requires a target.");
                }
                return false;
            }

            GatheringNodeComponent gatheringNodeComponent = GatheringNodeComponent.GetGatheringNodeComponent(target);
            if (gatheringNodeComponent == null) {
                //Debug.Log("You cannot use " + DisplayName + " on: " + target.name);
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". This ability must target a gathering node");
                }
                return false;
            }

            if (gatheringNodeComponent.GatheringNodeProps.BaseAbility == this) {
                return true;
            } else {
                //Debug.Log(target.name + " requires ability: " + _gatheringNode.MyAbility);
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". This gathering node requires the skill : " + gatheringNodeComponent.GatheringNodeProps.BaseAbility.DisplayName);
                }
                return false;
            }
        }

    }

}