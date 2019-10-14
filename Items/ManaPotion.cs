using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ManaPotion", menuName = "Inventory/Items/ManaPotion", order = 1)]
public class ManaPotion : CastableItem {

    public override void Use() {
        if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.currentMana < PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMaxMana) {
            Debug.Log("The current mana was less than the max mana and we can use the potion: " + this.GetInstanceID().ToString());
            base.Use();
        } else {
            MessageFeedManager.MyInstance.WriteMessage("Your mana is already full!");
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
