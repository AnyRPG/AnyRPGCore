using UnityEngine;

namespace AnyRPG {
    public class SerializableAbilityEffectContext {

        // keep track of damage / heal amounts for all resources
        //public List<ResourceInputAmountNode> resourceAmounts = new List<ResourceInputAmountNode>();

        public int overrideDuration = 0;
        public bool savedEffect = false;
        public float castTimeMultiplier = 1f;
        public float spellDamageMultiplier = 1f;

        // was this damage caused by a reflect?  Needed to stop infinite reflect loops
        public bool reflectDamage = false;

        public Vector3 groundTargetLocation = Vector3.zero;

        //public Interactable originalTarget;

        // track the ability that was originally cast that resulted in this effect
        public string baseAbilityName = string.Empty;

        // track the ability effect that caused this effect
        public string sourceAbilityEffectName = string.Empty;

        // the last power resource affected
        public string powerResourceName = string.Empty;

        // prevent multiple onHit effects from casting each other
        public bool weaponHitHasCast = false;

        // information about the original caster
        //private IAbilityCaster abilityCaster = null;
        public Vector3 abilityCasterLocation = Vector3.zero;
        public Quaternion abilityCasterRotation = Quaternion.identity;

        public SerializableAbilityEffectContext() {
        }

    }

}