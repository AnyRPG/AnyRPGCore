using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ManaPotion", menuName = "Inventory/Items/ManaPotion", order = 1)]
public class ManaPotion : Item, IUseable
{
    [SerializeField]
    private int mana;

    public override void Use() {
        base.Use();
        if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.currentMana < PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMaxMana) {
            Debug.Log("The current mana was less than the max mana and we can use the potion: " + this.GetInstanceID().ToString());
            Remove();
            PlayerManager.MyInstance.MyCharacter.MyCharacterStats.RecoverMana(mana, PlayerManager.MyInstance.MyCharacter);
        }
    }

    public override string GetSummary() {
        return base.GetSummary() + string.Format("\n<color=green>Use: Restores {0} mana</color>", mana);
    }
}
