using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public abstract class FixedLengthEffectProperties : LengthEffectProperties {

        /// <summary>
        /// the default amount of time after which we destroy any spawned prefab
        /// </summary>
        public float defaultPrefabLifetime = 10f;

        // game manager references
        protected SystemAbilityController systemAbilityController = null;

        public float AbilityEffectObjectLifetime { get => defaultPrefabLifetime; set => defaultPrefabLifetime = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

    }
}