using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "ManaPotion", menuName = "AnyRPG/Inventory/Items/ManaPotion", order = 1)]
    public class ManaPotion : CastableItem {

        public override bool Use() {
            if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.currentMana < PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMaxMana) {
                //Debug.Log("The current mana was less than the max mana and we can use the potion: " + this.GetInstanceID().ToString());
                bool returnValue = base.Use();
                if (returnValue == false) {
                    return false;
                }
                return returnValue;
            } else {
                MessageFeedManager.MyInstance.WriteMessage("Your mana is already full!");
                return false;
            }
        }

        public override string GetSummary() {
            string returnString = base.GetSummary();
            if (ability != null) {
                returnString += string.Format("\n<color=green>Use: {0}</color>", ability.MyDescription);
            }
            return returnString;
        }
    }

}