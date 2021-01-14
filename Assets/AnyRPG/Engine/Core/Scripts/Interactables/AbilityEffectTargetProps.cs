using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class AbilityEffectTargetProps: TargetProps {

        [Header("Line Of Sight Options")]

        [Tooltip("If line of sight is required, where should it be calculated from. Useful for splash damage and ground target explosions.")]
        [SerializeField]
        protected LineOfSightSourceLocation lineOfSightSourceLocation;

        [Tooltip("Where to calculate max range from.  Useful for splash damage and ground target explosions.")]
        [SerializeField]
        protected TargetRangeSourceLocation targetRangeSourceLocation;

        public override LineOfSightSourceLocation LineOfSightSourceLocation { get => lineOfSightSourceLocation; }
        public override TargetRangeSourceLocation TargetRangeSourceLocation { get => targetRangeSourceLocation; }



    }
}