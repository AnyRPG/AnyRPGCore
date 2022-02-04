using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class TauntEffectProperties : StatusEffectProperties {

        // extra threat from the taunt
        private float extraThreat = 100f;

        public float ExtraThreat { get => extraThreat; set => extraThreat = value; }
    }
}
