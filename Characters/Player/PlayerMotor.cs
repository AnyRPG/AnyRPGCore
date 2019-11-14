using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerMotor : CharacterMotor {

        protected override void Awake() {
            base.Awake();
        }

        protected override void Start() {
            //Debug.Log("PlayerMotor.Start()");
            base.Start();
            //(characterUnit.MyCharacter.MyCharacterController as PlayerController).OnManualMovement += StopFollowingTarget;
        }

        // Update is called once per frame
        protected override void FixedUpdate() {
            //Debug.Log("PlayerMotor.FixedUpdate()");
            base.FixedUpdate();
        }

        public override void Move(Vector3 moveDirection, bool isKnockBack = false) {
            if (isKnockBack) {
                PlayerUnitMovementController playerUnitMovementController = (characterUnit as MonoBehaviour).GetComponent<PlayerUnitMovementController>();
                if (playerUnitMovementController != null) {
                    playerUnitMovementController.KnockBack();
                }
            }
            base.Move(moveDirection, isKnockBack);
        }

        public void OnDestroy() {
            //(characterUnit.MyCharacter.MyCharacterController as PlayerController).OnManualMovement -= StopFollowingTarget;
        }
    }

}