using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public Interactable originalTarget;

        // track the ability that was originally cast that resulted in this effect
        public BaseAbilityProperties baseAbility = null;

        // the last power resource affected
        public PowerResource powerResource = null;

        // prevent multiple onHit effects from casting each other
        public bool weaponHitHasCast = false;

        // information about the original caster
        private IAbilityCaster abilityCaster = null;
        private Vector3 abilityCasterLocation = Vector3.zero;
        private Quaternion abilityCasterRotation;

        // these are intentionally not copied as we want them associated only with the ability effect that cast them
        private Dictionary<PrefabProfile, GameObject> prefabObjects = new Dictionary<PrefabProfile, GameObject>();

        public Dictionary<PrefabProfile, GameObject> PrefabObjects { get => prefabObjects; set => prefabObjects = value; }
        public IAbilityCaster AbilityCaster { get => abilityCaster; set => abilityCaster = value; }
        public Vector3 AbilityCasterLocation { get => abilityCasterLocation; set => abilityCasterLocation = value; }
        public Quaternion AbilityCasterRotation { get => abilityCasterRotation; set => abilityCasterRotation = value; }

        public AbilityEffectContext() {
        }

        public AbilityEffectContext(IAbilityCaster abilityCaster) {
            this.abilityCaster = abilityCaster;
            abilityCasterLocation = abilityCaster.transform.position;
            abilityCasterRotation = abilityCaster.transform.rotation;
        }

        public AbilityEffectContext GetCopy() {
            // make a new ability effect context
            AbilityEffectContext returnValue = new AbilityEffectContext();

            // copy all properties
            returnValue.abilityCaster = abilityCaster;
            returnValue.overrideDuration = overrideDuration;
            returnValue.savedEffect = savedEffect;
            returnValue.castTimeMultiplier = castTimeMultiplier;
            returnValue.spellDamageMultiplier = spellDamageMultiplier;
            returnValue.reflectDamage = reflectDamage;
            returnValue.groundTargetLocation = groundTargetLocation;
            returnValue.originalTarget = originalTarget;
            returnValue.baseAbility = baseAbility;
            returnValue.powerResource = powerResource;
            returnValue.weaponHitHasCast = weaponHitHasCast;
            returnValue.abilityCasterLocation = abilityCasterLocation;
            returnValue.abilityCasterRotation = abilityCasterRotation;

            // resource amounts must be copied.  ToList() or other methods don't work because you end up with a new list of references to the same old nodes
            returnValue.resourceAmounts = new List<ResourceInputAmountNode>();
            foreach (ResourceInputAmountNode resourceInputAmountNode in resourceAmounts) {
                returnValue.resourceAmounts.Add(new ResourceInputAmountNode(resourceInputAmountNode.resourceName, resourceInputAmountNode.amount));
            }

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