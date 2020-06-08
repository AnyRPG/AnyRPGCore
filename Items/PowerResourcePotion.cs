using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "PowerResourcePotion", menuName = "AnyRPG/Inventory/Items/PowerResourcePotion", order = 1)]
    public class PowerResourcePotion : CastableItem {

        [Header("Power Resource")]

        [Tooltip("The power resource to refill when this potion is used")]
        [SerializeField]
        private string powerResourceName = string.Empty;

        private PowerResource powerResource = null;

        public override bool Use() {
            if (PlayerManager.MyInstance.MyCharacter.CharacterStats.GetPowerResourceAmount(powerResource) < PlayerManager.MyInstance.MyCharacter.CharacterStats.GetPowerResourceMaxAmount(powerResource)) {
                //Debug.Log("The current resource amount was less than the max resource amount and we can use the potion: " + this.GetInstanceID().ToString());
                bool returnValue = base.Use();
                if (returnValue == false) {
                    return false;
                }
                return returnValue;
            } else {
                MessageFeedManager.MyInstance.WriteMessage("Your " + powerResource.MyDisplayName + " is already full!");
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

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (powerResourceName != null && powerResourceName != string.Empty) {
                PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(powerResourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find power resource : " + powerResourceName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                }
            }

        }
    }

}