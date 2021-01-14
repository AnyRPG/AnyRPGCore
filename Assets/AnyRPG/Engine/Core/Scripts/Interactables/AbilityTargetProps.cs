using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class AbilityTargetProps: TargetProps {

        [Header("Ground Target")]

        [Tooltip("If true, casting this spell will require choosing a target point on the ground, instead of a target character or object.")]
        [SerializeField]
        private bool requiresGroundTarget = false;

        [Tooltip("If this is a ground targeted spell, tint it with this color.")]
        [SerializeField]
        private Color groundTargetColor = new Color32(255, 255, 255, 255);

        [Tooltip("How big should the projector be on the ground if this is ground targeted. Used to show accurate effect size.")]
        [SerializeField]
        private float groundTargetRadius = 0f;

        public override bool RequiresGroundTarget { get => requiresGroundTarget; }
        public Color GroundTargetColor { get => groundTargetColor; }
        public float GroundTargetRadius { get => groundTargetRadius; }
    }
}