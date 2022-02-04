using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class AOEEffectProperties : FixedLengthEffectProperties {

        [Header("AOE")]

        [SerializeField]
        protected float aoeRadius;

        [SerializeField]
        protected bool useRadius = true;

        [SerializeField]
        protected bool useExtents = false;

        [SerializeField]
        protected Vector3 aoeCenter;

        [SerializeField]
        protected Vector3 aoeExtents;

        [SerializeField]
        protected float maxTargets = 0;

        [SerializeField]
        protected bool preferClosestTargets = false;

        // delay between casting hit effect on each target
        [SerializeField]
        protected float interTargetDelay = 0f;


    }

    public class AOETargetNode {
        public Interactable targetGameObject;
        public AbilityEffectContext abilityEffectInput;
    }

}