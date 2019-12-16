using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Animation Profile", menuName = "AnyRPG/Animation/Profile")]
    public class AnimationProfile : DescribableResource {

        /*
        [SerializeField]
        private string profileName;
    */
        [SerializeField]
        private bool suppressAdjustAnimatorSpeed = false;

        [SerializeField]
        private List<AnimationClip> animationClips = new List<AnimationClip>();

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
        private AnimationClip moveForwardClip;
        [SerializeField]
        private AnimationClip moveForwardFastClip;
        [SerializeField]
        private AnimationClip moveBackClip;
        [SerializeField]
        private AnimationClip moveBackFastClip;
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
        [SerializeField]
        private AnimationClip combatJumpClip;
        [SerializeField]
        private AnimationClip combatFallClip;
        [SerializeField]
        private AnimationClip combatLandClip;
        [SerializeField]
        private AnimationClip combatIdleClip;
        [SerializeField]
        private AnimationClip combatMoveForwardClip;
        [SerializeField]
        private AnimationClip combatMoveForwardFastClip;
        [SerializeField]
        private AnimationClip combatMoveBackClip;
        [SerializeField]
        private AnimationClip combatMoveBackFastClip;
        [SerializeField]
        private AnimationClip combatTurnLeftClip;
        [SerializeField]
        private AnimationClip combatTurnRightClip;
        [SerializeField]
        private AnimationClip combatStrafeLeftClip;
        [SerializeField]
        private AnimationClip combatJogStrafeLeftClip;
        [SerializeField]
        private AnimationClip combatStrafeRightClip;
        [SerializeField]
        private AnimationClip combatJogStrafeRightClip;
        [SerializeField]
        private AnimationClip combatStrafeForwardLeftClip;
        [SerializeField]
        private AnimationClip combatJogStrafeForwardLeftClip;
        [SerializeField]
        private AnimationClip combatStrafeForwardRightClip;
        [SerializeField]
        private AnimationClip combatJogStrafeForwardRightClip;
        [SerializeField]
        private AnimationClip combatStrafeBackLeftClip;
        [SerializeField]
        private AnimationClip combatJogStrafeBackLeftClip;
        [SerializeField]
        private AnimationClip combatStrafeBackRightClip;
        [SerializeField]
        private AnimationClip combatJogStrafeBackRightClip;
        [SerializeField]
        private AnimationClip combatStunnedClip;
        [SerializeField]
        private AnimationClip deathClip;
        [SerializeField]
        private AnimationClip reviveClip;
        [SerializeField]
        private AnimationClip levitatedClip;

        //public string MyProfileName { get => profileName; set => profileName = value; }
        public List<AnimationClip> MyAnimationClips { get => animationClips; set => animationClips = value; }
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
        public AnimationClip MyMoveBackFastClip { get => moveBackFastClip; set => moveBackFastClip = value; }
        public AnimationClip MyDeathClip { get => deathClip; set => deathClip = value; }
        public AnimationClip MyReviveClip { get => reviveClip; set => reviveClip = value; }
        public AnimationClip MyStunnedClip { get => stunnedClip; set => stunnedClip = value; }
        public AnimationClip MyLevitatedClip { get => stunnedClip; set => stunnedClip = value; }
        public AnimationClip MyJogStrafeLeftClip { get => jogStrafeLeftClip; set => jogStrafeLeftClip = value; }
        public AnimationClip MyJogStrafeRightClip { get => jogStrafeRightClip; set => jogStrafeRightClip = value; }
        public AnimationClip MyJogStrafeForwardLeftClip { get => jogStrafeForwardLeftClip; set => jogStrafeForwardLeftClip = value; }
        public AnimationClip MyJogStrafeForwardRightClip { get => jogStrafeForwardRightClip; set => jogStrafeForwardRightClip = value; }
        public AnimationClip MyJogStrafeBackLeftClip { get => jogStrafeBackLeftClip; set => jogStrafeBackLeftClip = value; }
        public AnimationClip MyJogStrafeBackRightClip { get => jogStrafeBackRightClip; set => jogStrafeBackRightClip = value; }
        public AnimationClip MyCombatTurnLeftClip { get => combatTurnLeftClip; set => combatTurnLeftClip = value; }
        public AnimationClip MyCombatTurnRightClip { get => combatTurnRightClip; set => combatTurnRightClip = value; }
        public AnimationClip MyCombatStrafeLeftClip { get => combatStrafeLeftClip; set => combatStrafeLeftClip = value; }
        public AnimationClip MyCombatJogStrafeLeftClip { get => combatJogStrafeLeftClip; set => combatJogStrafeLeftClip = value; }
        public AnimationClip MyCombatStrafeRightClip { get => combatStrafeRightClip; set => combatStrafeRightClip = value; }
        public AnimationClip MyCombatJogStrafeRightClip { get => combatJogStrafeRightClip; set => combatJogStrafeRightClip = value; }
        public AnimationClip MyCombatStrafeForwardLeftClip { get => combatStrafeForwardLeftClip; set => combatStrafeForwardLeftClip = value; }
        public AnimationClip MyCombatJogStrafeForwardLeftClip { get => combatJogStrafeForwardLeftClip; set => combatJogStrafeForwardLeftClip = value; }
        public AnimationClip MyCombatStrafeForwardRightClip { get => combatStrafeForwardRightClip; set => combatStrafeForwardRightClip = value; }
        public AnimationClip MyCombatJogStrafeForwardRightClip { get => combatJogStrafeForwardRightClip; set => combatJogStrafeForwardRightClip = value; }
        public AnimationClip MyCombatStrafeBackLeftClip { get => combatStrafeBackLeftClip; set => combatStrafeBackLeftClip = value; }
        public AnimationClip MyCombatJogStrafeBackLeftClip { get => combatJogStrafeBackLeftClip; set => combatJogStrafeBackLeftClip = value; }
        public AnimationClip MyCombatStrafeBackRightClip { get => combatStrafeBackRightClip; set => combatStrafeBackRightClip = value; }
        public AnimationClip MyCombatJogStrafeBackRightClip { get => combatJogStrafeBackRightClip; set => combatJogStrafeBackRightClip = value; }
        public AnimationClip MyCombatMoveForwardClip { get => combatMoveForwardClip; set => combatMoveForwardClip = value; }
        public AnimationClip MyCombatMoveForwardFastClip { get => combatMoveForwardFastClip; set => combatMoveForwardFastClip = value; }
        public AnimationClip MyCombatMoveBackClip { get => combatMoveBackClip; set => combatMoveBackClip = value; }
        public AnimationClip MyCombatMoveBackFastClip { get => combatMoveBackFastClip; set => combatMoveBackFastClip = value; }
        public AnimationClip MyCombatStunnedClip { get => combatStunnedClip; set => combatStunnedClip = value; }
        public AnimationClip MyCombatJumpClip { get => combatJumpClip; set => combatJumpClip = value; }
        public AnimationClip MyCombatFallClip { get => combatFallClip; set => combatFallClip = value; }
        public AnimationClip MyCombatLandClip { get => combatLandClip; set => combatLandClip = value; }
        public bool MySuppressAdjustAnimatorSpeed { get => suppressAdjustAnimatorSpeed; set => suppressAdjustAnimatorSpeed = value; }
    }

}