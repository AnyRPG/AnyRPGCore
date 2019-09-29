using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Animation Profile", menuName = "Animation/Profile")]
public class AnimationProfile : ScriptableObject {

    [SerializeField]
    private string profileName;

    [SerializeField]
    private AnimationProfileNode[] animationProfileNodes;
    [SerializeField]
    private AnimationClip[] takeDamageClips;
    [SerializeField]
    private AnimationClip jumpClip;
    [SerializeField]
    private AnimationClip fallClip;
    [SerializeField]
    private AnimationClip landClip;
    [SerializeField]
    private AnimationClip idleClip;
    [SerializeField]
    private AnimationClip combatIdleClip;
    [SerializeField]
    private AnimationClip turnLeftClip;
    [SerializeField]
    private AnimationClip turnRightClip;
    [SerializeField]
    private AnimationClip strafeLeftClip;
    [SerializeField]
    private AnimationClip jogStrafeLeftClip;
    [SerializeField]
    private AnimationClip strafeRightClip;
    [SerializeField]
    private AnimationClip jogStrafeRightClip;
    [SerializeField]
    private AnimationClip strafeForwardLeftClip;
    [SerializeField]
    private AnimationClip jogStrafeForwardLeftClip;
    [SerializeField]
    private AnimationClip strafeForwardRightClip;
    [SerializeField]
    private AnimationClip jogStrafeForwardRightClip;
    [SerializeField]
    private AnimationClip strafeBackLeftClip;
    [SerializeField]
    private AnimationClip strafeBackRightClip;
    [SerializeField]
    private AnimationClip moveForwardClip;
    [SerializeField]
    private AnimationClip moveForwardFastClip;
    [SerializeField]
    private AnimationClip moveBackClip;
    [SerializeField]
    private AnimationClip deathClip;
    [SerializeField]
    private AnimationClip reviveClip;
    [SerializeField]
    private AnimationClip stunnedClip;
    [SerializeField]
    private AnimationClip levitatedClip;

    public string MyProfileName { get => profileName; set => profileName = value; }
    public AnimationProfileNode[] MyProfileNodes { get => animationProfileNodes; set => animationProfileNodes = value; }
    public AnimationClip[] MyTakeDamageClips { get => takeDamageClips; set => takeDamageClips = value; }
    public AnimationClip MyJumpClip { get => jumpClip; set => jumpClip = value; }
    public AnimationClip MyFallClip { get => fallClip; set => fallClip = value; }
    public AnimationClip MyLandClip { get => landClip; set => landClip = value; }
    public AnimationClip MyIdleClip { get => idleClip; set => idleClip = value; }
    public AnimationClip MyCombatIdleClip { get => combatIdleClip; set => combatIdleClip = value; }
    public AnimationClip MyTurnLeftClip { get => turnLeftClip; set => turnLeftClip = value; }
    public AnimationClip MyTurnRightClip { get => turnRightClip; set => turnRightClip = value; }
    public AnimationClip MyStrafeLeftClip { get => strafeLeftClip; set => strafeLeftClip = value; }
    public AnimationClip MyStrafeRightClip { get => strafeRightClip; set => strafeRightClip = value; }
    public AnimationClip MyStrafeForwardLeftClip { get => strafeForwardLeftClip; set => strafeForwardLeftClip = value; }
    public AnimationClip MyStrafeForwardRightClip { get => strafeForwardRightClip; set => strafeForwardRightClip = value; }
    public AnimationClip MyStrafeBackLeftClip { get => strafeBackLeftClip; set => strafeBackLeftClip = value; }
    public AnimationClip MyStrafeBackRightClip { get => strafeBackRightClip; set => strafeBackRightClip = value; }
    public AnimationClip MyMoveForwardClip { get => moveForwardClip; set => moveForwardClip = value; }
    public AnimationClip MyMoveForwardFastClip { get => moveForwardFastClip; set => moveForwardFastClip = value; }
    public AnimationClip MyMoveBackClip { get => moveBackClip; set => moveBackClip = value; }
    public AnimationClip MyDeathClip { get => deathClip; set => deathClip = value; }
    public AnimationClip MyReviveClip { get => reviveClip; set => reviveClip = value; }
    public AnimationClip MyStunnedClip { get => stunnedClip; set => stunnedClip = value; }
    public AnimationClip MyLevitatedClip { get => stunnedClip; set => stunnedClip = value; }
    public AnimationClip MyJogStrafeLeftClip { get => jogStrafeLeftClip; set => jogStrafeLeftClip = value; }
    public AnimationClip MyJogStrafeRightClip { get => jogStrafeRightClip; set => jogStrafeRightClip = value; }
    public AnimationClip MyJogStrafeForwardLeftClip { get => jogStrafeForwardLeftClip; set => jogStrafeForwardLeftClip = value; }
    public AnimationClip MyJogStrafeForwardRightClip { get => jogStrafeForwardRightClip; set => jogStrafeForwardRightClip = value; }
}
