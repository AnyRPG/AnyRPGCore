using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New PetEffect", menuName = "AnyRPG/Abilities/Effects/PetEffect")]
    public class PetEffect : AbilityEffect {

        [SerializeField]
        private PetEffectProperties petEffectProperties = new PetEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => petEffectProperties; }



    }
}
