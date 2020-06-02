using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Power Resource", menuName = "AnyRPG/PowerResource")]
    [System.Serializable]
    public class PowerResource : DescribableResource {

        [Header("Display")]

        [Tooltip("The color that will be used for text and image backgrounds when an item of this quality is displayed")]
        [SerializeField]
        private Color displayColor;

        [Header("Regeneration")]

        [Tooltip("Every x seconds, perform the regeneration")]
        [SerializeField]
        private float tickRate = 1f;

        [Tooltip("The amount of this resource to regenerate per second while out of combat")]
        [SerializeField]
        private float regenPerTick = 0f;

        [Tooltip("If true, the regen per tick is a percentage of the maximum resource amount")]
        [SerializeField]
        private bool regenIsPercent = false;

        [Tooltip("The amount of this resource to regenerate per second while in combat")]
        [SerializeField]
        private float combatRegenPerTick = 0f;

        [Tooltip("If true, the combat regen per tick is a percentage of the maximum resource amount")]
        [SerializeField]
        private bool combatRegenIsPercent = false;

        [Header("Limits")]

        [Tooltip("If this amount is greater than zero, the resource has a maximum fixed amount")]
        [SerializeField]
        private float maximumAmount = 0f;

        [Header("Health")]

        [Tooltip("When all of a characters health resources have reached zero, they are considered to have died.  Multiple health resources are allowed.")]
        [SerializeField]
        private bool isHealth = false;


        public Color DisplayColor { get => displayColor; set => displayColor = value; }
        public float RegenPerTick { get => regenPerTick; set => regenPerTick = value; }
        public float CombatRegenPerTick { get => combatRegenPerTick; set => combatRegenPerTick = value; }
        public float MaximumAmount { get => maximumAmount; set => maximumAmount = value; }
        public float TickRate { get => tickRate; set => tickRate = value; }
        public bool RegenIsPercent { get => regenIsPercent; set => regenIsPercent = value; }
        public bool CombatRegenIsPercent { get => combatRegenIsPercent; set => combatRegenIsPercent = value; }
        public bool IsHealth { get => isHealth; set => isHealth = value; }
    }

}