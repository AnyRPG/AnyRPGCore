using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ProjectileEffect", menuName = "AnyRPG/Abilities/Effects/ProjectileEffect")]
    public class ProjectileEffect : AbilityEffect {

        [SerializeField]
        private ProjectileEffectProperties projectileEffectProperties = new ProjectileEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => projectileEffectProperties; }


    }
}