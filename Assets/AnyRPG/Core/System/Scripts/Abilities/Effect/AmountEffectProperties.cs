using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    
    // base class to hold amounts and spellpower calculations for heal and damage effects
    public abstract class AmountEffectProperties : InstantEffectProperties {

        [Header("Amounts")]

        [Tooltip("If true, this effect can do critical amounts")]
        [SerializeField]
        private bool allowCriticalStrike = true;

        [Tooltip("The resources to affect, and the amounts of the effects")]
        [SerializeField]
        private List<ResourceAmountNode> resourceAmounts = new List<ResourceAmountNode>();

        [SerializeField]
        protected DamageType damageType;

        [Header("Accuracy")]

        [Tooltip("If true, this effect will always hit regardless of current accuracy")]
        [SerializeField]
        protected bool ignoreAccuracy = false;

        public DamageType DamageType { get => damageType; set => damageType = value; }
        public bool AllowCriticalStrike { get => allowCriticalStrike; set => allowCriticalStrike = value; }
        public List<ResourceAmountNode> ResourceAmounts { get => resourceAmounts; set => resourceAmounts = value; }
        public bool IgnoreAccuracy { get => ignoreAccuracy; set => ignoreAccuracy = value; }
    }

    [System.Serializable]
    public class ResourceAmountNode {

        [Tooltip("The resource to add to or remove from")]
        [SerializeField]
        private string resourceName = string.Empty;

        private PowerResource powerResource = null;

        [Tooltip("If this is not empty, the resource amount will receive input from the following resource, instead of the resource with the same name in the ability effect context.")]
        [SerializeField]
        private string inputRemap = string.Empty;

        [Tooltip("If true, add the appropriate power (spell/physical) to this ability amount")]
        [SerializeField]
        private bool addPower = true;

        [Tooltip("If the amount is lower than this value, it will be rasied to this value.")]
        [SerializeField]
        private int minAmount = 0;

        [Tooltip("Amount not scaled by level.  This will be added to any scaled value.")]
        [SerializeField]
        private int baseAmount = 0;

        [Tooltip("This amount will be multipled by the caster level")]
        [SerializeField]
        private float amountPerLevel = 0f;

        [Tooltip("If the amount is higher than this value, it will be lowered to this value.  0 is unlimited.")]
        [SerializeField]
        private int maxAmount = 0;

        public string ResourceName { get => resourceName; set => resourceName = value; }
        public int MinAmount { get => minAmount; set => minAmount = value; }
        public int BaseAmount { get => baseAmount; set => baseAmount = value; }
        public float AmountPerLevel { get => amountPerLevel; set => amountPerLevel = value; }
        public int MaxAmount { get => maxAmount; set => maxAmount = value; }
        public PowerResource PowerResource { get => powerResource; set => powerResource = value; }
        public bool AddPower { get => addPower; set => addPower = value; }
        public string InputRemap { get => inputRemap; set => inputRemap = value; }

        public void SetupScriptableObjects(SystemDataFactory systemDataFactory) {

            if (resourceName != null && resourceName != string.Empty) {
                PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(resourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find power resource : " + resourceName + " while inititalizing statresourceNode.  CHECK INSPECTOR");
                }
            }

        }
    }

    public class ResourceInputAmountNode {

        public ResourceInputAmountNode(string resourceName, float resourceAmount) {
            this.resourceName = resourceName;
            amount = resourceAmount;
        }

        public string resourceName = string.Empty;

        public float amount = 0f;

    }
}