using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public abstract class StatusEffectBase : AbilityEffect {

        public override AbilityEffectProperties AbilityEffectProperties { get => StatusEffectProperties; }

        public virtual StatusEffectProperties StatusEffectProperties { get; }

    }


}
