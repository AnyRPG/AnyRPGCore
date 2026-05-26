using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ChanneledEffect",menuName = "AnyRPG/Abilities/Effects/ChanneledEffect")]
    public class ChanneledEffect : AbilityEffect {

        [SerializeField]
        public ChanneledEffectProperties channeledEffectProperties = new ChanneledEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => channeledEffectProperties; }



    }
}
