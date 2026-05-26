using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ProjectileEffect", menuName = "AnyRPG/Abilities/Effects/ProjectileEffect")]
    public class ProjectileEffect : AbilityEffect {

        [SerializeField]
        public ProjectileEffectProperties projectileEffectProperties = new ProjectileEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => projectileEffectProperties; }


    }
}