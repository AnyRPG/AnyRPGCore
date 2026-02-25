using UnityEngine;

namespace AnyRPG {
    public struct MovementData {
        public int frameCount;
        public bool inputJump;
        public bool inputFly;
        public bool inputSink;
        public bool inputStrafe;
        public bool inputCrouch;
        public float inputHorizontal;
        public float inputTurn;
        public float inputVertical;
        public Vector3 NormalizedMoveInput;
        public Vector3 TurnInput;

        public Vector3 LocalInput;
        public bool RightMouseButtonDown;
        public bool RightMouseDragged;
        public float RightAnalogHorizontal;
        public Vector3 CameraWantedDirection;
        public float CameraLocalEulerAngleX;
        public bool GamepadModeActive;

        public void ResetMoveInput() {
            frameCount = 0;
            inputJump = false;
            inputFly = false;
            inputSink = false;
            inputStrafe = false;
            inputCrouch = false;
            inputHorizontal = 0;
            inputVertical = 0;
            inputTurn = 0;

            RightMouseButtonDown = false;
            RightMouseDragged = false;
            RightAnalogHorizontal = 0;
            CameraLocalEulerAngleX = 0;
            LocalInput = Vector3.zero;

            NormalizedMoveInput = Vector3.zero;
            TurnInput = new Vector3(inputTurn, 0, 0);
        }

        public bool HasAnyInput() {
            if (NormalizedMoveInput != Vector3.zero || TurnInput != Vector3.zero || inputJump != false) {
                return true;
            } else {
                return false;
            }
        }

        public bool HasWaterMoveInput() {
            if (NormalizedMoveInput != Vector3.zero || TurnInput != Vector3.zero || inputSink != false || inputFly != false) {
                return true;
            } else {
                return false;
            }
        }

        public bool HasFlyMoveInput() {
            if (NormalizedMoveInput != Vector3.zero || TurnInput != Vector3.zero || inputSink != false || inputFly != false) {
                return true;
            } else {
                return false;
            }
        }

        public bool HasMoveInput() {
            if (NormalizedMoveInput != Vector3.zero) {
                return true;
            } else {
                return false;
            }
        }

        public bool HasTurnInput() {
            if (TurnInput != Vector3.zero) {
                return true;
            } else {
                return false;
            }
        }

    }
}    
