using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class CraftAbilityProperties : DirectAbilityProperties {

        // game manager references
        protected CraftingManager craftingManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            craftingManager = systemGameManager.CraftingManager;
        }

        public override List<AbilityAttachmentNode> GetHoldableObjectList(IAbilityCaster abilityCaster) {
            if (craftingManager.CraftingQueue.Count > 0) {
                List<AbilityAttachmentNode> returnList = new List<AbilityAttachmentNode>();
                foreach (AbilityAttachmentNode prefabProfile in base.GetHoldableObjectList(abilityCaster)) {
                    returnList.Add(prefabProfile);
                }
                foreach (AbilityAttachmentNode abilityAttachmentNode in craftingManager.CraftingQueue[0].HoldableObjectList) {
                    returnList.Add(abilityAttachmentNode);
                }
                return returnList;
            }
            return base.GetHoldableObjectList(abilityCaster);
        }

        public override bool Cast(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log("CraftAbility.Cast(" + (target ? target.name : "null") + ")");
            bool returnResult = base.Cast(source, target, abilityEffectContext);
            if (returnResult == true) {
                craftingManager.CraftNextItemWait();
            }
            return returnResult;
        }

        public override bool CanUseOn(Interactable target, IAbilityCaster source, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeChecks = true) {

            if (!base.CanUseOn(target, source, performCooldownChecks, abilityEffectContext, playerInitiated, performRangeChecks)) {
                return false;
            }

            // to prevent casting this ability on a valid crafting target from action bars with no recipe to make, it is not possible to cast if there is nothing in the queue
            if (craftingManager.CraftingQueue.Count == 0) {
                return false;
            }

            List<CraftingNodeComponent> craftingNodeComponents = CraftingNodeComponent.GetCraftingNodeComponents(target);
            if (craftingNodeComponents == null || craftingNodeComponents.Count == 0) {
                if (playerInitiated) {
                    source.AbilityManager.ReceiveCombatMessage("Cannot cast " + DisplayName + ". This ability must target a crafting node");
                }
                return false;
            }

            foreach (CraftingNodeComponent craftingNodeComponent in craftingNodeComponents) {
                if (craftingNodeComponent.Props.Ability == this) {
                    return true;
                }
            }

            //Debug.Log(target.name + " requires ability: " + _gatheringNode.MyAbility);
            if (playerInitiated) {
                source.AbilityManager.ReceiveCombatMessage("Cannot cast " + DisplayName + ". Target is not valid for this type of crafting");
            }
            return false;
        }

    }

}