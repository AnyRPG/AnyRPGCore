using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class CraftAbilityProperties : AbilityProperties {

        // game manager references
        protected CraftingManager craftingManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            craftingManager = systemGameManager.CraftingManager;
        }

        public override List<AbilityAttachmentNode> GetHoldableObjectList(IAbilityCaster abilityCaster) {
            //Debug.Log($"CraftAbilityProperties.GetHoldableObjectList({abilityCaster.gameObject.name})");

            if (abilityCaster.AbilityManager.GetCharacterUnit().UnitController.CharacterCraftingManager.CraftingQueue.Count > 0) {
                List<AbilityAttachmentNode> returnList = new List<AbilityAttachmentNode>();
                foreach (AbilityAttachmentNode abilityAttachmentNode in base.GetHoldableObjectList(abilityCaster)) {
                    returnList.Add(abilityAttachmentNode);
                }
                foreach (AbilityAttachmentNode abilityAttachmentNode in abilityCaster.AbilityManager.GetCharacterUnit().UnitController.CharacterCraftingManager.CraftingQueue[0].HoldableObjectList) {
                    returnList.Add(abilityAttachmentNode);
                }
                return returnList;
            }
            return base.GetHoldableObjectList(abilityCaster);
        }

        public override bool Cast(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"CraftAbility.Cast({source.gameObject.name}, {(target ? target.name : "null")})");

            bool returnResult = base.Cast(source, target, abilityEffectContext);
            if (returnResult == true) {
                source.AbilityManager.GetCharacterUnit().UnitController.CharacterCraftingManager.CraftNextItemWait();
            }
            return returnResult;
        }

        public override bool CanUseOn(Interactable target, IAbilityCaster source, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeChecks = true) {
            //Debug.Log($"CraftAbility.CanUseOn({source.gameObject.name}, {(target ? target.gameObject.name : "null")})");

            if (!base.CanUseOn(target, source, performCooldownChecks, abilityEffectContext, playerInitiated, performRangeChecks)) {
                //Debug.Log($"CraftAbility.CanUseOn({source.gameObject.name}, {(target ? target.gameObject.name : "null")}) base.CanUseOn failed");
                return false;
            }

            // to prevent casting this ability on a valid crafting target from action bars with no recipe to make, it is not possible to cast if there is nothing in the queue
            if (source.AbilityManager.GetCharacterUnit().UnitController.CharacterCraftingManager.CraftingQueue.Count == 0) {
                //Debug.Log($"CraftAbility.CanUseOn({source.gameObject.name}, {(target ? target.gameObject.name : "null")}) crafting queue is empty");
                return false;
            }

            List<CraftingNodeComponent> craftingNodeComponents = CraftingNodeComponent.GetCraftingNodeComponents(target);
            if (craftingNodeComponents == null || craftingNodeComponents.Count == 0) {
                if (playerInitiated) {
                    source.AbilityManager.ReceiveCombatMessage($"Cannot cast {DisplayName}. This ability must target a crafting node");
                }
                //Debug.Log($"CraftAbility.CanUseOn({source.gameObject.name}, {(target ? target.gameObject.name : "null")}) target does not have a crafting node component");
                return false;
            }

            foreach (CraftingNodeComponent craftingNodeComponent in craftingNodeComponents) {
                if (craftingNodeComponent.Props.Ability == this) {
                    return true;
                }
            }

            if (playerInitiated) {
                //Debug.Log($"CraftAbility.CanUseOn({source.gameObject.name}, {(target ? target.gameObject.name : "null")}) target does not require this ability");
                source.AbilityManager.ReceiveCombatMessage($"Cannot cast {DisplayName}. Target is not valid for this type of crafting");
            }
            return false;
        }

    }

}