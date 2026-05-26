using UnityEngine;

namespace AnyRPG {
    // NOTE: DIRECTEFFECT WILL CAST TICK AND COMPLETE, BUT NEVER HIT.  HIT MUST BE CAST BY PROJECTILE, AOE, OR CHANNELED
    [CreateAssetMenu(fileName = "New DirectEffect",menuName = "AnyRPG/Abilities/Effects/DirectEffect")]
    public class DirectEffect : AbilityEffect {

        [SerializeField]
        public DirectEffectProperties directEffectProperties = new DirectEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => directEffectProperties; }

        /*
        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            directEffectProperties.SetupScriptableObjects(systemGameManager);
        }
        */
    }

}