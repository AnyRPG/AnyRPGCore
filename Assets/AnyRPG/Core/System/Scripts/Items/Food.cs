using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Food", menuName = "AnyRPG/Inventory/Items/Food", order = 1)]
    public class Food : CastableItem {

        [Tooltip("The verb to use in the casting tip")]
        [SerializeField]
        private FoodConsumptionVerb consumptionVerb = FoodConsumptionVerb.Eat;

        [Tooltip("While consuming, the number of seconds between each resource refill")]
        [SerializeField]
        private float tickRate = 0f;

        [Tooltip("The resources to refill, and the amounts of the refill every tick")]
        [SerializeField]
        private List<PowerResourcePotionAmountNode> resourceAmounts = new List<PowerResourcePotionAmountNode>();

        [Tooltip("The status effect to cast if the food is completely consumed")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(StatusEffect))]
        private string statusEffect = string.Empty;

        public override string GetCastableInformation() {
            string returnString = string.Empty;
            if (ability != null) {
                returnString += string.Format("\n<color=green>Use: {0}</color>", ability.Description);
            }
            return returnString;
        }

    }

    public enum FoodConsumptionVerb { Eat, Drink }

}