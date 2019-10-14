using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Inventory/Items/Food", order = 1)]
public class Food : CastableItem {

    /*
    public override void Use() {
        //Debug.Log("Scroll.Use()");
        base.Use();
        PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
        Remove();
    }
    */

    public override string GetSummary() {
        string returnString = base.GetSummary();
        if (ability != null) {
            returnString += string.Format("\n<color=green>Use: {0}</color>", ability.MyDescription);
        }
        return returnString;
    }


}
