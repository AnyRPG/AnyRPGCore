using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AbilityCoolDownNode {

        private string abilityName = string.Empty;
        private Coroutine coroutine = null;
        private float remainingCoolDown = 0f;
        private float initialCoolDown = 0f;

        public string MyAbilityName { get => abilityName; set => abilityName = value; }
        public Coroutine MyCoroutine { get => coroutine; set => coroutine = value; }
        public float MyRemainingCoolDown { get => remainingCoolDown; set => remainingCoolDown = value; }
        public float MyInitialCoolDown { get => initialCoolDown; set => initialCoolDown = value; }
    }

}
