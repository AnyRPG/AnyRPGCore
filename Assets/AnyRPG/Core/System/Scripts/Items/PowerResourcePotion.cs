using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "PowerResourcePotion", menuName = "AnyRPG/Inventory/Items/PowerResourcePotion", order = 1)]
    public class PowerResourcePotion : ActionItem {

        [Header("Power Resource")]

        /*
        [Tooltip("The power resource to refill when this potion is used")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(PowerResource))]
        private string powerResourceName = string.Empty;
        */

        [Tooltip("The resources to affect, and the amounts of the effects")]
        [SerializeField]
        private List<PowerResourcePotionAmountNode> resourceAmounts = new List<PowerResourcePotionAmountNode>();

        private HealEffectProperties healEffect = null;

        public override bool Use() {
            //Debug.Log(DisplayName + ".PowerResourcePotion.Use()");
            int fullcount = 0;
            foreach (ResourceAmountNode resourceAmountNode in healEffect.ResourceAmounts) {
                if (playerManager.MyCharacter.CharacterStats.GetPowerResourceAmount(resourceAmountNode.PowerResource) >= playerManager.MyCharacter.CharacterStats.GetPowerResourceMaxAmount(resourceAmountNode.PowerResource)) {
                    fullcount++;
                }
            }
            if (fullcount >= healEffect.ResourceAmounts.Count) {
                //messageFeedManager.WriteMessage("Your " + powerResource.DisplayName + " is already full!");
                messageFeedManager.WriteMessage("Already full!");
                return false;
            }
            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }

            // perform heal effect
            healEffect.Cast(playerManager.ActiveCharacter, playerManager.UnitController, null, null);

            return returnValue;

        }

        /*
        public override string GetCastableInformation() {
            //Debug.Log(DisplayName + ".PowerResourcePotion.GetCastableInformation()");
            string returnString = string.Empty;
            //if (ability != null) {
                returnString += string.Format("\n<color=green>Use: {0}</color>", description);
            //}
            return returnString;
        }
        */

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            healEffect = new HealEffectProperties();
            healEffect.AllowCriticalStrike = false;

            // target options should not actually be necessary because they are bypassed when directly casted
            healEffect.TargetOptions.AutoSelfCast = true;
            healEffect.TargetOptions.CanCastOnOthers = false;
            healEffect.TargetOptions.CanCastOnSelf = true;
            healEffect.TargetOptions.RequireLiveTarget = true;
            healEffect.TargetOptions.RequireTarget = true;

            foreach (PowerResourcePotionAmountNode powerResourceAmountNode in resourceAmounts) {
                ResourceAmountNode resourceAmountNode = new ResourceAmountNode();
                resourceAmountNode.ResourceName = powerResourceAmountNode.ResourceName;
                resourceAmountNode.AddPower = false;
                resourceAmountNode.MinAmount = powerResourceAmountNode.MinAmount;
                resourceAmountNode.BaseAmount = powerResourceAmountNode.BaseAmount;
                resourceAmountNode.MaxAmount = powerResourceAmountNode.MaxAmount;
                healEffect.ResourceAmounts.Add(resourceAmountNode);
            }
            healEffect.SetupScriptableObjects(systemGameManager, this);

            /*
            if (powerResourceName != null && powerResourceName != string.Empty) {
                PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(powerResourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError("PowerResourcePotion.SetupScriptableObjects(): Could not find power resource : " + powerResourceName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }
            */

        }
    }

}