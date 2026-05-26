using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Pet Control Effect", menuName = "AnyRPG/Abilities/Effects/PetControlEffect")]
    public class PetControlEffect : AbilityEffect {

        [SerializeField]
        public PetControlEffectProperties petControlEffectProperties = new PetControlEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => petControlEffectProperties; }

       

    }
}
