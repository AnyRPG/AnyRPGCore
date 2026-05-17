using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


namespace AnyRPG {
    [CreateAssetMenu(fileName = "New MountEffect", menuName = "AnyRPG/Abilities/Effects/MountEffect")]
    public class MountEffect : StatusEffectBase {

        [SerializeField]
        public MountEffectProperties mountEffectProperties = new MountEffectProperties();

        public override StatusEffectProperties StatusEffectProperties { get => mountEffectProperties; }
    }
}
