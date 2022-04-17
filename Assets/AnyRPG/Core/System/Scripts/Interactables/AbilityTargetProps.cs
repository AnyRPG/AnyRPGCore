using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class AbilityTargetProps: TargetProps {

        [Header("Direction")]

        [Tooltip("If true, the target must be in front of the caster to cast this ability")]
        [SerializeField]
        private bool requireFacingTarget = false;

        [Tooltip("If Require Facing Target is true, the maximum angle allowed to be turned from straight")]
        [SerializeField]
        private float maxAngle = 45f;

        [Header("Position")]

        [Tooltip("If true, the caster must be behind the target to cast this ability")]
        [SerializeField]
        private bool requireBehindTarget = false;

        /*
        [Tooltip("If Require Facing Target is true, the maximum angle allowed to be turned from straight")]
        [SerializeField]
        private float maxBehindAngle = 45f;
        */

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

        private Vector3 localTargetPosition;
        private Vector3 localTargetDirection;
        private float localtargetAngle = 0f;

        public override bool RequiresGroundTarget { get => requiresGroundTarget; }
        public Color GroundTargetColor { get => groundTargetColor; }
        public float GroundTargetRadius { get => groundTargetRadius; }
        public bool RequireFacingTarget { get => requireFacingTarget; }
        public bool RequireBehindTarget { get => requireBehindTarget; }

        public override bool CanUseOn(ITargetable targetable, Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            //Debug.Log("AbilityTargetProps.CanUseOn()");
            if (base.CanUseOn(targetable, target, sourceCharacter, abilityEffectContext, playerInitiated, performRangeCheck) == false ) {
                return false;
            }

            if (requireFacingTarget) {
                if (target == null) {
                    return false;
                }
                localTargetPosition = sourceCharacter.AbilityManager.UnitGameObject.transform.InverseTransformPoint(target.transform.position);
                localTargetDirection = localTargetPosition.normalized;
                localtargetAngle = Vector3.Angle(Vector2.up, new Vector2(localTargetDirection.x, localTargetDirection.z));
                //Debug.Log("local target position: " + localTargetPosition + "; direction: " + localTargetDirection + "; angle: " + localtargetAngle);
                if (localTargetPosition.z < 0f || localtargetAngle > maxAngle) {
                    if (playerInitiated) {
                        sourceCharacter.AbilityManager.ReceiveMessageFeedMessage("You must be facing your target to cast " + targetable.DisplayName);
                    }
                    return false;
                }
            }

            if (requireBehindTarget) {
                if (target == null) {
                    return false;
                }
                if (target.transform.InverseTransformPoint(sourceCharacter.AbilityManager.UnitGameObject.transform.position).z >= 0f) {
                    if (playerInitiated) {
                        sourceCharacter.AbilityManager.ReceiveMessageFeedMessage("You must be behind your target to cast " + targetable.DisplayName);
                    }
                    return false;
                }
            }
            return true;
        }
    }
}