using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New SummonEffect", menuName = "AnyRPG/Abilities/Effects/SummonEffect")]
    public class SummonEffect : AbilityEffect {

        [SerializeField]
        public SummonEffectProperties summonEffectProperties = new SummonEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => summonEffectProperties; }



    }

}
