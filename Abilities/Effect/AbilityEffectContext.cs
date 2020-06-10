using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AbilityEffectContext {

        // keep track of damage / heal amounts for all resources
        public List<ResourceInputAmountNode> resourceAmounts = new List<ResourceInputAmountNode>();

        public int overrideDuration = 0;
        public bool savedEffect = false;
        public float castTimeMultiplier = 1f;
        public float spellDamageMultiplier = 1f;

        // was this damage caused by a reflect?  Needed to stop infinite reflect loops
        public bool reflectDamage = false;

        public Vector3 groundTargetLocation = Vector3.zero;

        public GameObject originalTarget;

        // track the ability that was originally cast that resulted in this effect
        public BaseAbility baseAbility = null;

        // the last power resource affected
        public PowerResource powerResource = null;

        public AbilityEffectContext GetCopy() {
            // make a new ability effect context
            AbilityEffectContext returnValue = new AbilityEffectContext();

            // copy all properties
            returnValue.resourceAmounts = resourceAmounts;
            returnValue.overrideDuration = overrideDuration;
            returnValue.savedEffect = savedEffect;
            returnValue.castTimeMultiplier = castTimeMultiplier;
            returnValue.spellDamageMultiplier = spellDamageMultiplier;
            returnValue.reflectDamage = reflectDamage;
            returnValue.groundTargetLocation = groundTargetLocation;
            returnValue.originalTarget = originalTarget;
            returnValue.baseAbility = baseAbility;
            returnValue.powerResource = powerResource;

            // return the new object
            return returnValue;
        }

        public void SetResourceAmount(string resourceName, float resourceValue) {
            bool foundResource = false;
            foreach (ResourceInputAmountNode resourceInputAmountNode in resourceAmounts) {
                if (resourceInputAmountNode.resourceName == resourceName) {
                    resourceInputAmountNode.amount = resourceValue;
                    foundResource = true;
                }
            }
            if (!foundResource) {
                resourceAmounts.Add(new ResourceInputAmountNode(resourceName, resourceValue));
            }
        }


        public void AddResourceAmount(string resourceName, float resourceValue) {
            //Debug.Log("AbilityEffectContext.AddResourceAmount(" + resourceName + ", " + resourceValue + ")");
            bool foundResource = false;
            foreach (ResourceInputAmountNode resourceInputAmountNode in resourceAmounts) {
                if (resourceInputAmountNode.resourceName == resourceName) {
                    resourceInputAmountNode.amount += resourceValue;
                    foundResource = true;
                }
            }
            if (!foundResource) {
                resourceAmounts.Add(new ResourceInputAmountNode(resourceName, resourceValue));
            }
        }
    }

}