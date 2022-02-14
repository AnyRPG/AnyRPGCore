using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // base class to hold amounts and spellpower calculations for heal and damage effects
    public abstract class AmountEffect : InstantEffect {
        /*
        [Header("Amounts")]

        [Tooltip("If true, this effect can do critical amounts")]
        [SerializeField]
        protected bool allowCriticalStrike = true;

        [Tooltip("The resources to affect, and the amounts of the effects")]
        [SerializeField]
        protected List<ResourceAmountNode> resourceAmounts = new List<ResourceAmountNode>();

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
        */

    }

}