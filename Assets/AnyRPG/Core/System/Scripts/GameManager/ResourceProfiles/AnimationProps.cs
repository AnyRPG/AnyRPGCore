using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [System.Serializable]
    public class AnimationProps {

        [Header("Animation Options")]

        [SerializeField]
        private bool useRootMotion = false;

        [SerializeField]
        private bool suppressAdjustAnimatorSpeed = false;

        [Header("Actions")]

        [FormerlySerializedAs("animationClips")]
        [SerializeField]
        private List<AnimationClip> attackClips = new List<AnimationClip>();

        [SerializeField]
        private List<AnimationClip> castClips = new List<AnimationClip>();

        [SerializeField]
        private List<AnimationClip> takeDamageClips = new List<AnimationClip>();

        [SerializeField]
        private List<AnimationClip> actionClips = new List<AnimationClip>();

        [Header("Out Of Combat Movement")]

        [SerializeField]
        private AnimationClip idleClip;
        [SerializeField]
        private AnimationClip jumpClip;
        [SerializeField]
        private AnimationClip fallClip;
        [SerializeField]
        private AnimationClip landClip;
        
        [FormerlySerializedAs("moveForwardClip")]
        [SerializeField]
        private AnimationClip walkClip;

        [FormerlySerializedAs("moveForwardFastClip")]
        [SerializeField]
        private AnimationClip runClip;

        [FormerlySerializedAs("moveBackClip")]
        [SerializeField]
        private AnimationClip walkBackClip;

        [FormerlySerializedAs("moveBackFastClip")]
        [SerializeField]
        private AnimationClip runBackClip;

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
        private AnimationClip jogStrafeBackLeftClip;
        [SerializeField]
        private AnimationClip strafeBackRightClip;
        [SerializeField]
        private AnimationClip jogStrafeBackRightClip;
        [SerializeField]
        private AnimationClip stunnedClip;

        [Header("Combat Movement")]

        [Tooltip("If true, all combat clips will mirror their non combat versions, regardless of the individual boxes checked in the combat movement section.")]
        [SerializeField]
        private bool fullCombatMirror = false;

        [Tooltip("If true, the combat idle clip will use the non combat idle clip")]
        [SerializeField]
        private bool combatIdleMirror = false;

        [SerializeField]
        private AnimationClip combatIdleClip;

        [Tooltip("If true, the combat jump clip will use the non combat jump clip")]
        [SerializeField]
        private bool combatJumpMirror = false;

        [SerializeField]
        private AnimationClip combatJumpClip;

        [Tooltip("If true, the combat fall clip will use the non combat fall clip")]
        [SerializeField]
        private bool combatFallMirror = false;

        [SerializeField]
        private AnimationClip combatFallClip;

        [Tooltip("If true, the combat land clip will use the non combat land clip")]
        [SerializeField]
        private bool combatLandMirror = false;

        [SerializeField]
        private AnimationClip combatLandClip;

        [Tooltip("If true, the combat move forward clip will use the non combat move forward clip")]
        [SerializeField]
        private bool combatWalkMirror = false;

        [FormerlySerializedAs("combatMoveForwardClip")]
        [SerializeField]
        private AnimationClip combatWalkClip;

        [Tooltip("If true, the combat move forward fast clip will use the non combat move forward fast clip")]
        [SerializeField]
        private bool combatRunMirror = false;

        [FormerlySerializedAs("combatMoveForwardFastClip")]
        [SerializeField]
        private AnimationClip combatRunClip;

        [Tooltip("If true, the combat move back clip will use the non combat move back clip")]
        [SerializeField]
        private bool combatWalkBackMirror = false;

        [FormerlySerializedAs("combatMoveBackClip")]
        [SerializeField]
        private AnimationClip combatWalkBackClip;

        [Tooltip("If true, the combat move back fast clip will use the non combat move back fast clip")]
        [SerializeField]
        private bool combatRunBackMirror = false;

        [FormerlySerializedAs("combatMoveBackFastClip")]
        [SerializeField]
        private AnimationClip combatRunBackClip;

        [Tooltip("If true, the combat turn left clip will use the non combat turn left clip")]
        [SerializeField]
        private bool combatTurnLeftMirror = false;

        [SerializeField]
        private AnimationClip combatTurnLeftClip;

        [Tooltip("If true, the combat turn right clip will use the non combat turn right clip")]
        [SerializeField]
        private bool combatTurnRightMirror = false;

        [SerializeField]
        private AnimationClip combatTurnRightClip;

        [Tooltip("If true, the combat strafe left clip will use the non combat strafe left clip")]
        [SerializeField]
        private bool combatStrafeLeftMirror = false;

        [SerializeField]
        private AnimationClip combatStrafeLeftClip;

        [Tooltip("If true, the combat jog strafe left clip will use the non combat jog strafe left clip")]
        [SerializeField]
        private bool combatJogStrafeLeftMirror = false;

        [SerializeField]
        private AnimationClip combatJogStrafeLeftClip;

        [Tooltip("If true, the combat strafe right clip will use the non combat strafe right clip")]
        [SerializeField]
        private bool combatStrafeRightMirror = false;

        [SerializeField]
        private AnimationClip combatStrafeRightClip;

        [Tooltip("If true, the combat jog strafe right clip will use the non combat jog strafe right clip")]
        [SerializeField]
        private bool combatJogStrafeRightMirror = false;

        [SerializeField]
        private AnimationClip combatJogStrafeRightClip;

        [Tooltip("If true, the combat strafe forward left clip will use the non combat strafe forward left clip")]
        [SerializeField]
        private bool combatStrafeForwardLeftMirror = false;

        [SerializeField]
        private AnimationClip combatStrafeForwardLeftClip;

        [Tooltip("If true, the combat jog strafe forward left clip will use the non combat jog strafe forward left clip")]
        [SerializeField]
        private bool combatJogStrafeForwardLeftMirror = false;

        [SerializeField]
        private AnimationClip combatJogStrafeForwardLeftClip;

        [Tooltip("If true, the combat strafe forward right clip will use the non combat strafe forward right clip")]
        [SerializeField]
        private bool combatStrafeForwardRightMirror = false;

        [SerializeField]
        private AnimationClip combatStrafeForwardRightClip;

        [Tooltip("If true, the combat jog strafe forward right clip will use the non combat jog strafe forward right clip")]
        [SerializeField]
        private bool combatJogStrafeForwardRightMirror = false;

        [SerializeField]
        private AnimationClip combatJogStrafeForwardRightClip;

        [Tooltip("If true, the combat strafe back left clip will use the non combat strafe back left clip")]
        [SerializeField]
        private bool combatStrafeBackLeftMirror = false;

        [SerializeField]
        private AnimationClip combatStrafeBackLeftClip;

        [Tooltip("If true, the combat jog strafe back left clip will use the non combat jog strafe back left clip")]
        [SerializeField]
        private bool combatJogStrafeBackLeftMirror = false;

        [SerializeField]
        private AnimationClip combatJogStrafeBackLeftClip;

        [Tooltip("If true, the combat strafe back right clip will use the non combat strafe back right clip")]
        [SerializeField]
        private bool combatStrafeBackRightMirror = false;

        [SerializeField]
        private AnimationClip combatStrafeBackRightClip;

        [Tooltip("If true, the combat jog strafe back right clip will use the non combat jog strafe back right clip")]
        [SerializeField]
        private bool combatJogStrafeBackRightMirror = false;

        [SerializeField]
        private AnimationClip combatJogStrafeBackRightClip;

        [Tooltip("If true, the combat stunned clip will use the non combat stunned clip")]
        [SerializeField]
        private bool combatStunnedMirror = false;

        [SerializeField]
        private AnimationClip combatStunnedClip;

        [Header("Common Clips")]

        [SerializeField]
        private AnimationClip deathClip;
        [SerializeField]
        private AnimationClip reviveClip;
        [SerializeField]
        private AnimationClip levitatedClip;

        [Header("Swimming")]

        [SerializeField]
        private AnimationClip swimIdleClip;

        [SerializeField]
        private AnimationClip swimMoveClip;

        [Header("Flying")]

        [SerializeField]
        private AnimationClip flyIdleClip;

        [SerializeField]
        private AnimationClip flyMoveClip;

        private Dictionary<string, AnimationClip> animationClips = new Dictionary<string, AnimationClip>();

        public List<AnimationClip> AttackClips { get => attackClips; set => attackClips = value; }
        public List<AnimationClip> CastClips { get => castClips; set => castClips = value; }
        public List<AnimationClip> TakeDamageClips { get => takeDamageClips; set => takeDamageClips = value; }
        public List<AnimationClip> ActionClips { get => actionClips; set => actionClips = value; }
        public AnimationClip IdleClip { get => idleClip; set => idleClip = value; }
        public AnimationClip JumpClip { get => jumpClip; set => jumpClip = value; }
        public AnimationClip FallClip { get => fallClip; set => fallClip = value; }
        public AnimationClip LandClip { get => landClip; set => landClip = value; }
        public AnimationClip TurnLeftClip { get => turnLeftClip; set => turnLeftClip = value; }
        public AnimationClip TurnRightClip { get => turnRightClip; set => turnRightClip = value; }
        public AnimationClip StrafeLeftClip { get => strafeLeftClip; set => strafeLeftClip = value; }
        public AnimationClip StrafeRightClip { get => strafeRightClip; set => strafeRightClip = value; }
        public AnimationClip StrafeForwardLeftClip { get => strafeForwardLeftClip; set => strafeForwardLeftClip = value; }
        public AnimationClip StrafeForwardRightClip { get => strafeForwardRightClip; set => strafeForwardRightClip = value; }
        public AnimationClip StrafeBackLeftClip { get => strafeBackLeftClip; set => strafeBackLeftClip = value; }
        public AnimationClip StrafeBackRightClip { get => strafeBackRightClip; set => strafeBackRightClip = value; }
        public AnimationClip WalkClip { get => walkClip; set => walkClip = value; }
        public AnimationClip RunClip { get => runClip; set => runClip = value; }
        public AnimationClip WalkBackClip { get => walkBackClip; set => walkBackClip = value; }
        public AnimationClip RunBackClip { get => runBackClip; set => runBackClip = value; }
        public AnimationClip StunnedClip { get => stunnedClip; set => stunnedClip = value; }
        public AnimationClip JogStrafeLeftClip { get => jogStrafeLeftClip; set => jogStrafeLeftClip = value; }
        public AnimationClip JogStrafeRightClip { get => jogStrafeRightClip; set => jogStrafeRightClip = value; }
        public AnimationClip JogStrafeForwardLeftClip { get => jogStrafeForwardLeftClip; set => jogStrafeForwardLeftClip = value; }
        public AnimationClip JogStrafeForwardRightClip { get => jogStrafeForwardRightClip; set => jogStrafeForwardRightClip = value; }
        public AnimationClip JogStrafeBackLeftClip { get => jogStrafeBackLeftClip; set => jogStrafeBackLeftClip = value; }
        public AnimationClip JogStrafeBackRightClip { get => jogStrafeBackRightClip; set => jogStrafeBackRightClip = value; }

        public AnimationClip CombatIdleClip {
            get {
                if (fullCombatMirror == true || combatIdleMirror == true) {
                    return idleClip;
                }
                return combatIdleClip;
            }
            set => combatIdleClip = value;
        }
        public AnimationClip CombatJumpClip {
            get {
                if (fullCombatMirror == true || combatJumpMirror == true) {
                    return jumpClip;
                }
                return combatJumpClip;
            }
            set => combatJumpClip = value;
        }
        public AnimationClip CombatFallClip {
            get {
                if (fullCombatMirror == true || combatFallMirror == true) {
                    return fallClip;
                }
                return combatFallClip;
            }
            set => combatFallClip = value;
        }
        public AnimationClip CombatLandClip {
            get {
                if (fullCombatMirror == true || combatLandMirror == true) {
                    return landClip;
                }
                return combatLandClip;
            }
            set => combatLandClip = value;
        }
        public AnimationClip CombatWalkClip {
            get {
                if (fullCombatMirror == true || combatWalkMirror == true) {
                    return walkClip;
                }
                return combatWalkClip;
            }
            set => combatWalkClip = value;
        }
        public AnimationClip CombatRunClip {
            get {
                if (fullCombatMirror == true || combatRunMirror == true) {
                    return runClip;
                }
                return combatRunClip;
            }
            set => combatRunClip = value;
        }
        public AnimationClip CombatWalkBackClip {
            get {
                if (fullCombatMirror == true || combatWalkBackMirror) {
                    return walkBackClip;
                }
                return combatWalkBackClip;
            }
            set => combatWalkBackClip = value;
        }
        public AnimationClip CombatRunBackClip {
            get {
                if (fullCombatMirror == true || combatRunBackMirror) {
                    return runBackClip;
                }
                return combatRunBackClip;
            }
            set => combatRunBackClip = value;
        }
        public AnimationClip CombatTurnLeftClip {
            get {
                if (fullCombatMirror == true || combatTurnLeftMirror) {
                    return turnLeftClip;
                }
                return combatTurnLeftClip;
            }
            set => combatTurnLeftClip = value;
        }
        public AnimationClip CombatTurnRightClip {
            get {
                if (fullCombatMirror == true || combatTurnRightMirror) {
                    return turnRightClip;
                }
                return combatTurnRightClip;
            }
            set => combatTurnRightClip = value;
        }
        public AnimationClip CombatStrafeLeftClip {
            get {
                if (fullCombatMirror == true || combatStrafeLeftMirror) {
                    return strafeLeftClip;
                }
                return combatStrafeLeftClip;
            }
            set => combatStrafeLeftClip = value;
        }
        public AnimationClip CombatJogStrafeLeftClip {
            get {
                if (fullCombatMirror == true || combatJogStrafeLeftMirror) {
                    return jogStrafeLeftClip;
                }
                return combatJogStrafeLeftClip;
            }
            set => combatJogStrafeLeftClip = value;
        }
        public AnimationClip CombatStrafeRightClip {
            get {
                if (fullCombatMirror == true || combatStrafeRightMirror) {
                    return strafeRightClip;
                }
                return combatStrafeRightClip;
            }
            set => combatStrafeRightClip = value;
        }
        public AnimationClip CombatJogStrafeRightClip {
            get {
                if (fullCombatMirror == true || combatJogStrafeRightMirror) {
                    return jogStrafeRightClip;
                }
                return combatJogStrafeRightClip;
            }
            set => combatJogStrafeRightClip = value;
        }
        public AnimationClip CombatStrafeForwardLeftClip {
            get {
                if (fullCombatMirror == true || combatStrafeForwardLeftMirror) {
                    return StrafeForwardLeftClip;
                }
                return combatStrafeForwardLeftClip;
            }
            set => combatStrafeForwardLeftClip = value;
        }
        public AnimationClip CombatJogStrafeForwardLeftClip {
            get {
                if (fullCombatMirror == true || combatJogStrafeForwardLeftMirror) {
                    return jogStrafeForwardLeftClip;
                }
                return combatJogStrafeForwardLeftClip;
            }
            set => combatJogStrafeForwardLeftClip = value;
        }
        public AnimationClip CombatStrafeForwardRightClip {
            get {
                if (fullCombatMirror == true || combatStrafeForwardRightMirror) {
                    return strafeForwardRightClip;
                }
                return combatStrafeForwardRightClip;
            }
            set => combatStrafeForwardRightClip = value;
        }
        public AnimationClip CombatJogStrafeForwardRightClip {
            get {
                if (fullCombatMirror == true || combatJogStrafeForwardRightMirror) {
                    return jogStrafeForwardRightClip;
                }
                return combatJogStrafeForwardRightClip;
            }
            set => combatJogStrafeForwardRightClip = value;
        }
        public AnimationClip CombatStrafeBackLeftClip {
            get {
                if (fullCombatMirror == true || combatStrafeBackLeftMirror) {
                    return StrafeBackLeftClip;
                }
                return combatStrafeBackLeftClip;
            }
            set => combatStrafeBackLeftClip = value;
        }
        public AnimationClip CombatJogStrafeBackLeftClip {
            get {
                if (fullCombatMirror == true || combatJogStrafeBackLeftMirror) {
                    return JogStrafeBackLeftClip;
                }
                return combatJogStrafeBackLeftClip;
            }
            set => combatJogStrafeBackLeftClip = value;
        }
        public AnimationClip CombatStrafeBackRightClip {
            get {
                if (fullCombatMirror == true || combatStrafeBackRightMirror) {
                    return strafeBackRightClip;
                }
                return combatStrafeBackRightClip;
            }
            set => combatStrafeBackRightClip = value;
        }
        public AnimationClip CombatJogStrafeBackRightClip {
            get {
                if (fullCombatMirror == true || combatJogStrafeBackRightMirror) {
                    return jogStrafeBackRightClip;
                }
                return combatJogStrafeBackRightClip;
            }
            set => combatJogStrafeBackRightClip = value;
        }
        public AnimationClip CombatStunnedClip {
            get {
                if (fullCombatMirror == true || combatStunnedMirror) {
                    return stunnedClip;
                }
                return combatStunnedClip;
            }
            set => combatStunnedClip = value;
        }
        public AnimationClip DeathClip { get => deathClip; set => deathClip = value; }
        public AnimationClip ReviveClip { get => reviveClip; set => reviveClip = value; }
        public AnimationClip LevitatedClip { get => levitatedClip; set => levitatedClip = value; }

        public AnimationClip SwimIdleClip { get => swimIdleClip; set => swimIdleClip = value; }
        public AnimationClip SwimMoveClip { get => swimMoveClip; set => swimMoveClip = value; }

        public AnimationClip FlyIdleClip { get => flyIdleClip; set => flyIdleClip = value; }
        public AnimationClip FlyMoveClip { get => flyMoveClip; set => flyMoveClip = value; }

        public bool SuppressAdjustAnimatorSpeed { get => suppressAdjustAnimatorSpeed; set => suppressAdjustAnimatorSpeed = value; }
        public bool UseRootMotion { get => useRootMotion; set => useRootMotion = value; }
        public Dictionary<string, AnimationClip> AnimationClips { get => animationClips; }

        public void Configure() {
            animationClips.Add("IdleClip", IdleClip);
            animationClips.Add("JumpClip", JumpClip);
            animationClips.Add("FallClip", FallClip);
            animationClips.Add("LandClip", LandClip);
            animationClips.Add("TurnLeftClip", TurnLeftClip);
            animationClips.Add("TurnRightClip", TurnRightClip);
            animationClips.Add("WalkClip", WalkClip);
            animationClips.Add("RunClip", RunClip);
            animationClips.Add("WalkBackClip", WalkBackClip);
            animationClips.Add("RunBackClip", RunBackClip);
            animationClips.Add("StrafeLeftClip", StrafeLeftClip);
            animationClips.Add("StrafeRightClip", StrafeRightClip);
            animationClips.Add("StrafeForwardLeftClip", StrafeForwardLeftClip);
            animationClips.Add("StrafeForwardRightClip", StrafeForwardRightClip);
            animationClips.Add("StrafeBackLeftClip", StrafeBackLeftClip);
            animationClips.Add("StrafeBackRightClip", StrafeBackRightClip);
            animationClips.Add("JogStrafeLeftClip", JogStrafeLeftClip);
            animationClips.Add("JogStrafeRightClip", JogStrafeRightClip);
            animationClips.Add("JogStrafeForwardLeftClip", JogStrafeForwardLeftClip);
            animationClips.Add("JogStrafeForwardRightClip", JogStrafeForwardRightClip);
            animationClips.Add("JogStrafeBackLeftClip", JogStrafeBackLeftClip);
            animationClips.Add("JogStrafeBackRightClip", JogStrafeBackRightClip);
            animationClips.Add("StunnedClip", StunnedClip);
            animationClips.Add("CombatIdleClip", CombatIdleClip);
            animationClips.Add("CombatJumpClip", CombatJumpClip);
            animationClips.Add("CombatFallClip", CombatFallClip);
            animationClips.Add("CombatLandClip", CombatLandClip);
            animationClips.Add("CombatWalkClip", CombatWalkClip);
            animationClips.Add("CombatRunClip", CombatRunClip);
            animationClips.Add("CombatWalkBackClip", CombatWalkBackClip);
            animationClips.Add("CombatRunBackClip", CombatRunBackClip);
            animationClips.Add("CombatTurnLeftClip", CombatTurnLeftClip);
            animationClips.Add("CombatTurnRightClip", CombatTurnRightClip);
            animationClips.Add("CombatStrafeLeftClip", CombatStrafeLeftClip);
            animationClips.Add("CombatJogStrafeLeftClip", CombatJogStrafeLeftClip);
            animationClips.Add("CombatStrafeRightClip", CombatStrafeRightClip);
            animationClips.Add("CombatJogStrafeRightClip", CombatJogStrafeRightClip);
            animationClips.Add("CombatStrafeForwardLeftClip", CombatStrafeForwardLeftClip);
            animationClips.Add("CombatJogStrafeForwardLeftClip", CombatJogStrafeForwardLeftClip);
            animationClips.Add("CombatStrafeForwardRightClip", CombatStrafeForwardRightClip);
            animationClips.Add("CombatJogStrafeForwardRightClip", CombatJogStrafeForwardRightClip);
            animationClips.Add("CombatStrafeBackLeftClip", CombatStrafeBackLeftClip);
            animationClips.Add("CombatJogStrafeBackLeftClip", CombatJogStrafeBackLeftClip);
            animationClips.Add("CombatStrafeBackRightClip", CombatStrafeBackRightClip);
            animationClips.Add("CombatJogStrafeBackRightClip", CombatJogStrafeBackRightClip);
            animationClips.Add("CombatStunnedClip", CombatStunnedClip);
            animationClips.Add("DeathClip", DeathClip);
            animationClips.Add("ReviveClip", ReviveClip);
            animationClips.Add("LevitatedClip", LevitatedClip);
            animationClips.Add("SwimIdleClip", SwimIdleClip);
            animationClips.Add("SwimMoveClip", SwimMoveClip);
            animationClips.Add("FlyIdleClip", FlyIdleClip);
            animationClips.Add("FlyMoveClip", FlyMoveClip);
        }
    }

}