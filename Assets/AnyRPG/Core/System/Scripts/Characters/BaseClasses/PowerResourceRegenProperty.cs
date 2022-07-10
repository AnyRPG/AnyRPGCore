using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    
    [System.Serializable]
    public class PowerResourceRegenProperty {

        [Tooltip("The resource to regenerate")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(PowerResource))]
        private string powerResource;

        [Header("Out Of Combat")]

        [Tooltip("The percentage of the resource to regenerate per point of character stat per tick while out of combat")]
        [SerializeField]
        private float percentPerTick = 0f;

        [Tooltip("The amount of the resource to regenerate point of character stat per tick while out of combat")]
        [SerializeField]
        private float amountPerTick = 0f;

        [Header("In Combat")]

        [Tooltip("The percentage of the resource to regenerate point of character stat per tick while in combat")]
        [SerializeField]
        private float combatPercentPerTick = 0f;

        [Tooltip("The percentage of the resource to regenerate point of character stat per tick while in combat")]
        [SerializeField]
        private float combatAmountPerTick = 0f;

        public string PowerResource { get => powerResource; set => powerResource = value; }
        public float PercentPerTick { get => percentPerTick; set => percentPerTick = value; }
        public float AmountPerTick { get => amountPerTick; set => amountPerTick = value; }
        public float CombatPercentPerTick { get => combatPercentPerTick; set => combatPercentPerTick = value; }
        public float CombatAmountPerTick { get => combatAmountPerTick; set => combatAmountPerTick = value; }
    }

}