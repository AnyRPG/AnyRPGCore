using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Scroll", menuName = "AnyRPG/Inventory/Items/Scroll", order = 1)]
    public class Scroll : CastableItem {

        public override string GetCastableInformation() {
            string abilityName = string.Empty;
            if (ability != null) {
                abilityName = ability.DisplayName;
            }
            return string.Format("\n<color=green>Use: Cast {0}</color>", abilityName);
        }


    }

}