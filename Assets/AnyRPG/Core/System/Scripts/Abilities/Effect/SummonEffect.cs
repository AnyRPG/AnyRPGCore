using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New SummonEffect", menuName = "AnyRPG/Abilities/Effects/SummonEffect")]
    public class SummonEffect : InstantEffect {
        /*
        [Header("Summon")]

        [Tooltip("Unit Prefab Profile to use for the summon pet")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private string unitProfileName = string.Empty;

        // reference to actual unitProfile
        private UnitProfile unitProfile = null;

        // reference to spawned object UnitController
        private UnitController petUnitController;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public string UnitProfileName { get => unitProfileName; set => unitProfileName = value; }
        */

        [SerializeField]
        private SummonEffectProperties summonEffectProperties = new SummonEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => summonEffectProperties; }



    }

}
