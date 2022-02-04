using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class ChanneledEffectProperties : DirectEffectProperties {

        // the amount of time to delay damage after spawning the prefab
        public float effectDelay = 0f;

    }
}
