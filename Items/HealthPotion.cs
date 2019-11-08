using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "HealthPotion", menuName = "AnyRPG/Inventory/Items/HealthPotion", order = 1)]
    public class HealthPotion : CastableItem {

        public override void Use() {
            if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.currentHealth < PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMaxHealth) {
                //Debug.Log("The current health was less than the max health and we can use the potion: " + this.GetInstanceID().ToString());
                base.Use();
            } else {
                MessageFeedManager.MyInstance.WriteMessage("Your health is already full!");
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