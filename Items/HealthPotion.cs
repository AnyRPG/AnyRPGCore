using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealthPotion", menuName = "Inventory/Items/HealthPotion", order = 1)]
public class HealthPotion : Item, IUseable
{
    [SerializeField]
    private int health;

    public override void Use() {
        base.Use();
        if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.currentHealth < PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMaxHealth) {
            Debug.Log("The current health was less than the max health and we can use the potion: " + this.GetInstanceID().ToString());
            PlayerManager.MyInstance.MyCharacter.MyCharacterStats.RecoverHealth(health, PlayerManager.MyInstance.MyCharacter);
            Remove();
        }
    }

    public override string GetSummary() {
        return base.GetSummary() + string.Format("\n<color=green>Use: Restores {0} health</color>", health);
    }

}
