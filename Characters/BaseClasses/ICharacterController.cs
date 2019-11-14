using AnyRPG;
using System;
using UnityEngine;

namespace AnyRPG {
    public interface ICharacterController {
        event System.Action<GameObject> OnSetTarget;
        event System.Action OnClearTarget;
        event System.Action OnManualMovement;

        ICharacter MyBaseCharacter { get; }
        GameObject MyTarget { get; }
        float MyMovementSpeed { get; }
        bool MyUnderControl { get; set; }
        BaseCharacter MyMasterUnit { get; set; }
        Vector3 MyLastPosition { get; set; }
        float MyApparentVelocity { get; set; }

        void ClearTarget();
        bool IsTargetInHitBox(GameObject newTarget);
        void SetTarget(GameObject newTarget);
        void FreezeCharacter();
        void UnFreezeCharacter();
        void StunCharacter();
        void UnStunCharacter();
        void LevitateCharacter();
        void UnLevitateCharacter();
        void Agro(CharacterUnit agroTarget);
    }
}