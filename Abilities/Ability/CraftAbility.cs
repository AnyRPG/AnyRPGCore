using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Craft Ability", menuName = "Abilities/Effects/CraftAbility")]
    public class CraftAbility : DirectAbility {

        public override bool Cast(BaseCharacter source, GameObject target, Vector3 groundTarget) {
            //Debug.Log("DirectAbility.Cast(" + (target ? target.name : "null") + ")");
            bool returnResult = base.Cast(source, target, groundTarget);
            if (returnResult == true) {
                CraftingUI.MyInstance.CraftNextItem();
            }
            return returnResult;
        }

    }

}