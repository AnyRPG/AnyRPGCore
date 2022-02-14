using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class AOEEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private AOEEffectProperties effectProperties = new AOEEffectProperties();

        public AbilityEffectProperties EffectProperties { get => effectProperties; }
    }

}