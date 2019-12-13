using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Scroll",menuName = "AnyRPG/Inventory/Items/Scroll", order = 1)]
    public abstract class CastableItem : Item, IUseable {

        [SerializeField]
        protected string abilityName;

        //[SerializeField]
        protected BaseAbility ability;

        public override bool Use() {
            //Debug.Log("CastableItem.Use()");
            if (ability == null) {
                Debug.LogError(MyName + ".CastableItem.Use(): ability is null.  Please set it in the inspector!");
                return false;
            }
            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }
            PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
            Remove();
            return returnValue;
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
        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            ability = null;
            if (abilityName != null) {
                BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(abilityName);
                if (baseAbility != null) {
                    ability = baseAbility;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability : " + abilityName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }
        }


    }

}