using AnyRPG;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ResurrectEffect", menuName = "AnyRPG/Abilities/Effects/ResurrectEffect")]
    public class ResurrectEffect : AbilityEffect {

        [SerializeField]
        private ResurrectEffectProperties resurrectEffectProperties = new ResurrectEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => resurrectEffectProperties; }



    }
}