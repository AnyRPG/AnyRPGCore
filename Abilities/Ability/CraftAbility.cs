using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Craft Ability",menuName = "AnyRPG/Abilities/Effects/CraftAbility")]
    public class CraftAbility : DirectAbility {

        public override List<AbilityAttachmentNode> HoldableObjectList {
            get {
                if (CraftingUI.MyInstance.CraftingQueue.Count > 0) {
                    List<AbilityAttachmentNode> returnList = new List<AbilityAttachmentNode>();
                    foreach (AbilityAttachmentNode prefabProfile in base.HoldableObjectList) {
                        returnList.Add(prefabProfile);
                    }
                    foreach (AbilityAttachmentNode abilityAttachmentNode in CraftingUI.MyInstance.CraftingQueue[0].HoldableObjectList) {
                        returnList.Add(abilityAttachmentNode);
                    }
                    return returnList;
                }
                return base.HoldableObjectList;
            }
            set => base.HoldableObjectList = value;
        }

        public override bool Cast(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log("CraftAbility.Cast(" + (target ? target.name : "null") + ")");
            bool returnResult = base.Cast(source, target, abilityEffectContext);
            if (returnResult == true) {
                CraftingUI.MyInstance.CraftNextItemWait();
            }
            return returnResult;
        }

    }

}