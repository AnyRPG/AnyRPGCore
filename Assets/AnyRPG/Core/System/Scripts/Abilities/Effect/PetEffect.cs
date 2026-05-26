using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New PetEffect", menuName = "AnyRPG/Abilities/Effects/PetEffect")]
    public class PetEffect : AbilityEffect {

        [SerializeField]
        public PetEffectProperties petEffectProperties = new PetEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => petEffectProperties; }



    }
}
