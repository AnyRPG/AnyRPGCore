using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Scroll",menuName = "AnyRPG/Inventory/Items/Scroll", order = 1)]
    public abstract class CastableItem : Item, IUseable {

        [SerializeField]
        protected BaseAbility ability;

        public override void Use() {
            //Debug.Log("CastableItem.Use()");
            if (ability == null) {
                Debug.LogError(MyName + ".CastableItem.Use(): ability is null.  Please set it in the inspector!");
                return;
            }
            base.Use();
            PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
            Remove();
        }

        /*
        public override string GetSummary() {
            string abilityName = "Ability Not Set In Inspector!";
            if (ability != null) {
                abilityName = ability.MyName;
            }
            return string.Format("{0}\n<color=green>Use: Cast {1}</color>", base.GetSummary(), abilityName);
        }
        */

    }

}