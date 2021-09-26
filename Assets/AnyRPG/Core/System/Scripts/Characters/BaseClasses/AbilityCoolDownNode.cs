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

        public string AbilityName { get => abilityName; set => abilityName = value; }
        public Coroutine Coroutine { get => coroutine; set => coroutine = value; }
        public float RemainingCoolDown { get => remainingCoolDown; set => remainingCoolDown = value; }
        public float InitialCoolDown { get => initialCoolDown; set => initialCoolDown = value; }
    }

}
