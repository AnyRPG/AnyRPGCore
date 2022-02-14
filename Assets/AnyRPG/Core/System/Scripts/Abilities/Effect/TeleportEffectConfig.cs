using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class TeleportEffectConfig : AbilityEffectConfig {


        [SerializeField]
        private TeleportEffectProperties effectProperties = new TeleportEffectProperties();

        public AbilityEffectProperties EffectProperties { get => effectProperties; }
    }

}
