using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


namespace AnyRPG {
    [CreateAssetMenu(fileName = "New MountEffect", menuName = "AnyRPG/Abilities/Effects/MountEffect")]
    public class MountEffect : StatusEffect {

        /*
        [Header("Mount")]

        [Tooltip("Unit Prefab Profile to use for the mount object")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private string unitProfileName = string.Empty;

        // reference to actual unitProfile
        private UnitProfile unitProfile = null;

        public string UnitProfileName { get => unitProfileName; set => unitProfileName = value; }
        */
        [SerializeField]
        private MountEffectProperties mountEffectProperties = new MountEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => mountEffectProperties; }




    }
}
