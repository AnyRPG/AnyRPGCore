using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Food", menuName = "AnyRPG/Inventory/Items/Food", order = 1)]
    public class Food : CastableItem {

        public override string GetCastableInformation() {
            string returnString = string.Empty;
            if (ability != null) {
                returnString += string.Format("\n<color=green>Use: {0}</color>", ability.MyDescription);
            }
            return returnString;
        }

    }

}