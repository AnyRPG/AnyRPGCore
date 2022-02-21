using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Pet Control Effect", menuName = "AnyRPG/Abilities/Effects/PetControlEffect")]
    public class PetControlEffect : AbilityEffect {

        [SerializeField]
        private PetControlEffectProperties petControlEffectProperties = new PetControlEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => petControlEffectProperties; }

       

    }
}
