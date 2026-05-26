using UnityEngine;


namespace AnyRPG {
    [CreateAssetMenu(fileName = "New MountEffect", menuName = "AnyRPG/Abilities/Effects/MountEffect")]
    public class MountEffect : StatusEffectBase {

        [SerializeField]
        public MountEffectProperties mountEffectProperties = new MountEffectProperties();

        public override StatusEffectProperties StatusEffectProperties { get => mountEffectProperties; }
    }
}
