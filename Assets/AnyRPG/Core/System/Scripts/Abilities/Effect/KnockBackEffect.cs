using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    
    [CreateAssetMenu(fileName = "New KnockBackEffect", menuName = "AnyRPG/Abilities/Effects/KnockBackEffect")]
    [System.Serializable]
    public class KnockBackEffect : AbilityEffect {

        [SerializeField]
        private KnockBackEffectProperties knockBackEffectProperties = new KnockBackEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => knockBackEffectProperties; }

    }

}
