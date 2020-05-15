using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "HealthPotion", menuName = "AnyRPG/Inventory/Items/HealthPotion", order = 1)]
    public class HealthPotion : CastableItem {

        public override bool Use() {
            if (PlayerManager.MyInstance.MyCharacter.CharacterStats.currentHealth < PlayerManager.MyInstance.MyCharacter.CharacterStats.MyMaxHealth) {
                //Debug.Log("The current health was less than the max health and we can use the potion: " + this.GetInstanceID().ToString());
                bool returnValue = base.Use();
                if (returnValue == false) {
                    return false;
                }
                return returnValue;
            } else {
                MessageFeedManager.MyInstance.WriteMessage("Your health is already full!");
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