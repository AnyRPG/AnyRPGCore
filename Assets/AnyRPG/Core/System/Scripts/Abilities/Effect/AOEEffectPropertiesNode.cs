using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class AOEEffectPropertiesNode {

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

        public float AoeRadius { get => aoeRadius; set => aoeRadius = value; }
        public bool UseRadius { get => useRadius; set => useRadius = value; }
        public bool UseExtents { get => useExtents; set => useExtents = value; }
        public Vector3 AoeCenter { get => aoeCenter; set => aoeCenter = value; }
        public Vector3 AoeExtents { get => aoeExtents; set => aoeExtents = value; }
        public float MaxTargets { get => maxTargets; set => maxTargets = value; }
        public bool PreferClosestTargets { get => preferClosestTargets; set => preferClosestTargets = value; }
        public float InterTargetDelay { get => interTargetDelay; set => interTargetDelay = value; }
    }

}