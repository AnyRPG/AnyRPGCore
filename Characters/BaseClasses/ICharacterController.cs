using System;
using UnityEngine;

public interface ICharacterController {
    event System.Action<GameObject> OnSetTarget;
    event System.Action OnClearTarget;

    ICharacter MyBaseCharacter { get; }
    GameObject MyTarget { get; }
    float MyMovementSpeed { get; }
    bool MyUnderControl { get; set; }
    BaseCharacter MyMasterUnit { get; set; }


    void ClearTarget();
    bool IsTargetInHitBox(GameObject newTarget);
    void SetTarget(GameObject newTarget);
    void FreezeCharacter();
    void UnFreezeCharacter();
    void StunCharacter();
    void UnStunCharacter();
    void LevitateCharacter();
    void UnLevitateCharacter();
}