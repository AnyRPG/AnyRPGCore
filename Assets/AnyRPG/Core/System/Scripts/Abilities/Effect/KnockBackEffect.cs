using UnityEngine;

namespace AnyRPG {
    
    [CreateAssetMenu(fileName = "New KnockBackEffect", menuName = "AnyRPG/Abilities/Effects/KnockBackEffect")]
    [System.Serializable]
    public class KnockBackEffect : AbilityEffect {

        [SerializeField]
        public KnockBackEffectProperties knockBackEffectProperties = new KnockBackEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => knockBackEffectProperties; }

    }

}
