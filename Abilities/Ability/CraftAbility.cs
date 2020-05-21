using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Craft Ability",menuName = "AnyRPG/Abilities/Effects/CraftAbility")]
    public class CraftAbility : DirectAbility {

        public override List<PrefabProfile> MyHoldableObjects {
            get {
                if (CraftingUI.MyInstance.CraftingQueue.Count > 0) {
                    List<PrefabProfile> returnList = new List<PrefabProfile>();
                    foreach (PrefabProfile prefabProfile in base.MyHoldableObjects) {
                        returnList.Add(prefabProfile);
                    }
                    foreach (PrefabProfile prefabProfile in CraftingUI.MyInstance.CraftingQueue[0].HoldableObjects) {
                        returnList.Add(prefabProfile);
                    }
                    return returnList;
                }
                return base.MyHoldableObjects;
            }
            set => base.MyHoldableObjects = value;
        }

        public override bool Cast(IAbilityCaster source, GameObject target, Vector3 groundTarget) {
            //Debug.Log("CraftAbility.Cast(" + (target ? target.name : "null") + ")");
            bool returnResult = base.Cast(source, target, groundTarget);
            if (returnResult == true) {
                CraftingUI.MyInstance.CraftNextItemWait();
            }
            return returnResult;
        }

    }

}