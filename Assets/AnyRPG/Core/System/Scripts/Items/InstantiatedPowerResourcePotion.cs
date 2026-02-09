using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class InstantiatedPowerResourcePotion : InstantiatedActionItem {

        private PowerResourcePotion powerResourcePotion = null;

        public InstantiatedPowerResourcePotion(SystemGameManager systemGameManager, long instanceId, PowerResourcePotion powerResourcePotion, ItemQuality itemQuality) : base(systemGameManager, instanceId, powerResourcePotion, itemQuality) {
            this.powerResourcePotion = powerResourcePotion;
        }

        public override bool Use(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.InstantiatedPowerResourcePotion.Use({sourceUnitController.gameObject.name})");

            int fullcount = 0;
            foreach (ResourceAmountNode resourceAmountNode in powerResourcePotion.HealEffect.ResourceAmounts) {
                if (sourceUnitController.CharacterStats.GetPowerResourceAmount(resourceAmountNode.PowerResource) >= sourceUnitController.CharacterStats.GetPowerResourceMaxAmount(resourceAmountNode.PowerResource)) {
                    fullcount++;
                }
            }
            if (fullcount >= powerResourcePotion.HealEffect.ResourceAmounts.Count) {
                //messageFeedManager.WriteMessage("Your " + powerResource.DisplayName + " is already full!");
                sourceUnitController.WriteMessageFeedMessage("Already full!");
                return false;
            }
            bool returnValue = base.Use(sourceUnitController);
            if (returnValue == false) {
                return false;
            }

            // perform heal effect
            powerResourcePotion.HealEffect.Cast(sourceUnitController, sourceUnitController, null, null);

            return returnValue;
        }

       
    }

}