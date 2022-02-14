using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New RemoveEffect", menuName = "AnyRPG/Abilities/Effects/RemoveEffect")]
    public class RemoveEffect : InstantEffect {
        /*
        // 0 is unlimited
        [SerializeField]
        private int maxClearEffects = 0;

        // default will only clear harmful effects

        // effect types that this ability can clear
        [SerializeField]
        [ResourceSelector(resourceType = typeof(StatusEffectType))]
        private List<string> effectTypeNames = new List<string>();

        private List<StatusEffectType> effectTypes = new List<StatusEffectType>();

        public int MaxClearEffects { get => maxClearEffects; set => maxClearEffects = value; }
        public List<string> EffectTypeNames { get => effectTypeNames; set => effectTypeNames = value; }
        */

        [SerializeField]
        private RemoveEffectProperties removeEffectProperties = new RemoveEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => removeEffectProperties; }


    }
}