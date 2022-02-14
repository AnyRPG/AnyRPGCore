using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Ability",menuName = "AnyRPG/Abilities/Effects/FixedLengthEffect")]
    // not using that for now as it will neither tick, nor complete.  that is done by directeffect/children or aoeEffect
    public abstract class FixedLengthEffect : LengthEffect {

        /*
        /// <summary>
        /// the default amount of time after which we destroy any spawned prefab
        /// </summary>
        public float defaultPrefabLifetime = 10f;

        // game manager references
        protected SystemAbilityController systemAbilityController = null;

        public float AbilityEffectObjectLifetime { get => defaultPrefabLifetime; set => defaultPrefabLifetime = value; }
        */

       

    }
}