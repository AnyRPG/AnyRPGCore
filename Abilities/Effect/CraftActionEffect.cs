using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New CraftActionEffect", menuName = "Abilities/Effects/CraftActionEffect")]
    public class CraftActionEffect : InstantEffect {

        public override void Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log("CraftAction.Cast()");
            base.Cast(source, target, originalTarget, abilityEffectInput);
            CraftingUI.MyInstance.CraftNextItem();
        }

    }

}
