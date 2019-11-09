﻿using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AbilityCoolDownNode : MonoBehaviour {

        private string abilityName = string.Empty;
        private Coroutine coroutine = null;
        private float remainingCoolDown = 0f;

        public string MyAbilityName { get => abilityName; set => abilityName = value; }
        public Coroutine MyCoroutine { get => coroutine; set => coroutine = value; }
        public float MyRemainingCoolDown { get => remainingCoolDown; set => remainingCoolDown = value; }
    }

}
