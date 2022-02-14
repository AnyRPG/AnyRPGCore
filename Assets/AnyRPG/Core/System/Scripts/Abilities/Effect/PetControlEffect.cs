using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Pet Control Effect", menuName = "AnyRPG/Abilities/Effects/PetControlEffect")]
    public class PetControlEffect : StatusEffect {

        /*
        [SerializeField]
        [ResourceSelector(resourceType = typeof(SummonEffect))]
        private List<string> petEffectNames = new List<string>();

        private List<SummonEffect> petEffectList = new List<SummonEffect>();

        public List<string> PetEffectNames { get => petEffectNames; set => petEffectNames = value; }
        */

        [SerializeField]
        private PetControlEffectProperties petControlEffectProperties = new PetControlEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => petControlEffectProperties; }

       

    }
}
