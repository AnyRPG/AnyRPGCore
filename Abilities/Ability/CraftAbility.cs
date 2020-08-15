using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Craft Ability",menuName = "AnyRPG/Abilities/Effects/CraftAbility")]
    public class CraftAbility : DirectAbility {

        public override List<PrefabProfile> HoldableObjects {
            get {
                if (CraftingUI.MyInstance.CraftingQueue.Count > 0) {
                    List<PrefabProfile> returnList = new List<PrefabProfile>();
                    foreach (PrefabProfile prefabProfile in base.HoldableObjects) {
                        returnList.Add(prefabProfile);
                    }
                    foreach (PrefabProfile prefabProfile in CraftingUI.MyInstance.CraftingQueue[0].HoldableObjects) {
                        returnList.Add(prefabProfile);
                    }
                    return returnList;
                }
                return base.HoldableObjects;
            }
            set => base.HoldableObjects = value;
        }

        public override bool Cast(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log("CraftAbility.Cast(" + (target ? target.name : "null") + ")");
            bool returnResult = base.Cast(source, target, abilityEffectContext);
            if (returnResult == true) {
                CraftingUI.MyInstance.CraftNextItemWait();
            }
            return returnResult;
        }

    }

}