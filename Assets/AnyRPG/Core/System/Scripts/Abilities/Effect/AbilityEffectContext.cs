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
        public float SpellDamageMultiplier = 1f;

        // was this damage caused by a reflect?  Needed to stop infinite reflect loops
        public bool ReflectDamage = false;

        public Vector3 GroundTargetLocation = Vector3.zero;

        public Interactable OriginalTarget;

        // track the ability that was originally cast that resulted in this effect
        public AbilityProperties BaseAbility = null;

        // track the ability effect that caused this effect
        public AbilityEffectProperties AbilityEffect = null;

        // the last power resource affected
        public PowerResource PowerResource = null;

        // prevent multiple onHit effects from casting each other
        public bool WeaponHitHasCast = false;

        // information about the original caster
        private IAbilityCaster abilityCaster = null;
        private Vector3 abilityCasterLocation = Vector3.zero;
        private Quaternion abilityCasterRotation;

        // these are intentionally not copied as we want them associated only with the ability effect that cast them
        private Dictionary<PrefabProfile, List<GameObject>> prefabObjects = new Dictionary<PrefabProfile, List<GameObject>>();

        public Dictionary<PrefabProfile, List<GameObject>> PrefabObjects { get => prefabObjects; set => prefabObjects = value; }
        public IAbilityCaster AbilityCaster { get => abilityCaster; set => abilityCaster = value; }
        public Vector3 AbilityCasterLocation { get => abilityCasterLocation; set => abilityCasterLocation = value; }
        public Quaternion AbilityCasterRotation { get => abilityCasterRotation; set => abilityCasterRotation = value; }

        public AbilityEffectContext() {
        }

        public AbilityEffectContext(AbilityEffectProperties abilityEffectProperties) {
            this.AbilityEffect = abilityEffectProperties;
        }

        public AbilityEffectContext(IAbilityCaster abilityCaster) {
            this.abilityCaster = abilityCaster;
            abilityCasterLocation = abilityCaster.transform.position;
            abilityCasterRotation = abilityCaster.transform.rotation;
        }

        public AbilityEffectContext(IAbilityCaster abilityCaster, Interactable originalTarget, SerializableAbilityEffectContext serializableAbilityEffectContext, SystemGameManager systemGameManager) {
            this.abilityCaster = abilityCaster;
            abilityCasterLocation = abilityCaster.transform.position;
            abilityCasterRotation = abilityCaster.transform.rotation;
            overrideDuration = serializableAbilityEffectContext.overrideDuration;
            savedEffect = serializableAbilityEffectContext.savedEffect;
            castTimeMultiplier = serializableAbilityEffectContext.castTimeMultiplier;
            SpellDamageMultiplier = serializableAbilityEffectContext.spellDamageMultiplier;
            ReflectDamage = serializableAbilityEffectContext.reflectDamage;
            GroundTargetLocation = serializableAbilityEffectContext.groundTargetLocation;
            this.OriginalTarget = originalTarget;
            BaseAbility = serializableAbilityEffectContext.baseAbilityName == string.Empty ? null : systemGameManager.SystemDataFactory.GetResource<Ability>(serializableAbilityEffectContext.baseAbilityName)?.AbilityProperties;
            PowerResource = serializableAbilityEffectContext.powerResourceName == string.Empty ? null : systemGameManager.SystemDataFactory.GetResource<PowerResource>(serializableAbilityEffectContext.powerResourceName);
            WeaponHitHasCast = serializableAbilityEffectContext.weaponHitHasCast;
            AbilityEffect = serializableAbilityEffectContext.sourceAbilityEffectName == string.Empty ? null : systemGameManager.SystemDataFactory.GetResource<AbilityEffect>(serializableAbilityEffectContext.sourceAbilityEffectName).AbilityEffectProperties;
        }

        public AbilityEffectContext GetCopy() {
            // make a new ability effect context
            AbilityEffectContext returnValue = new AbilityEffectContext();

            // copy all properties
            returnValue.abilityCaster = abilityCaster;
            returnValue.overrideDuration = overrideDuration;
            returnValue.savedEffect = savedEffect;
            returnValue.castTimeMultiplier = castTimeMultiplier;
            returnValue.SpellDamageMultiplier = SpellDamageMultiplier;
            returnValue.ReflectDamage = ReflectDamage;
            returnValue.GroundTargetLocation = GroundTargetLocation;
            returnValue.OriginalTarget = OriginalTarget;
            returnValue.BaseAbility = BaseAbility;
            returnValue.PowerResource = PowerResource;
            returnValue.WeaponHitHasCast = WeaponHitHasCast;
            returnValue.abilityCasterLocation = abilityCasterLocation;
            returnValue.abilityCasterRotation = abilityCasterRotation;
            returnValue.AbilityEffect = AbilityEffect;

            // resource amounts must be copied.  ToList() or other methods don't work because you end up with a new list of references to the same old nodes
            returnValue.resourceAmounts = new List<ResourceInputAmountNode>();
            foreach (ResourceInputAmountNode resourceInputAmountNode in resourceAmounts) {
                returnValue.resourceAmounts.Add(new ResourceInputAmountNode(resourceInputAmountNode.resourceName, resourceInputAmountNode.amount));
            }

            // return the new object
            return returnValue;
        }

        public SerializableAbilityEffectContext GetSerializableContext() {
            // make a new ability effect context
            SerializableAbilityEffectContext returnValue = new SerializableAbilityEffectContext();

            // copy all properties
            returnValue.overrideDuration = overrideDuration;
            returnValue.savedEffect = savedEffect;
            returnValue.castTimeMultiplier = castTimeMultiplier;
            returnValue.spellDamageMultiplier = SpellDamageMultiplier;
            returnValue.reflectDamage = ReflectDamage;
            returnValue.groundTargetLocation = GroundTargetLocation;
            returnValue.baseAbilityName = (BaseAbility != null ? BaseAbility.ResourceName : string.Empty);
            returnValue.powerResourceName = (PowerResource != null ? PowerResource.ResourceName : string.Empty);
            returnValue.weaponHitHasCast = WeaponHitHasCast;
            returnValue.abilityCasterLocation = abilityCasterLocation;
            returnValue.abilityCasterRotation = abilityCasterRotation;
            returnValue.sourceAbilityEffectName = (AbilityEffect != null ? AbilityEffect.ResourceName : string.Empty);

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