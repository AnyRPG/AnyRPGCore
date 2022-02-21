using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Capture Pet Effect", menuName = "AnyRPG/Abilities/Effects/CapturePetEffect")]
    public class CapturePetEffect : AbilityEffect {

        [SerializeField]
        private CapturePetEffectProperties capturePetEffectProperties = new CapturePetEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => capturePetEffectProperties; }


    }
}