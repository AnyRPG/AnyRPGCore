using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scroll", menuName = "Inventory/Items/Scroll", order = 1)]
public class Scroll : Item, IUseable
{
    [SerializeField]
    private BaseAbility ability;

    public override void Use() {
        base.Use();
        PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
        Remove();
    }

    public override string GetSummary() {
        string abilityName = "Ability Not Set In Inspector!";
        if (ability != null) {
            abilityName = ability.MyName;
        }
        return string.Format("{0}\n<color=green>Use: Cast {1}</color>", base.GetSummary(), abilityName);
    }


}
