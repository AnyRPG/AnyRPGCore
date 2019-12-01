using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AbilityEffectOutput {

        public int healthAmount = 0;
        public int manaAmount = 0;
        public int overrideDuration = 0;
        public bool savedEffect = false;
        public float castTimeMultipler = 1f;
        public float spellDamageMultiplier = 1f;

        public Vector3 prefabLocation = Vector3.zero;
    }

}