using UnityEngine;

namespace AnyRPG {
    public struct MovementData {
        public int FrameCount;
        public bool InputJump;
        public bool InputFly;
        public bool InputSink;
        public bool InputStrafe;
        public bool InputCrouch;
        public float InputHorizontal;
        public float InputTurn;
        public float InputVertical;
        public Vector3 NormalizedMoveInput;
        public Vector3 TurnInput;

        public Vector3 IntendedLocalDirection;
        public Vector3 IntendedWorldDirection;
        public bool RightMouseButtonDown;
        public bool RightMouseDragged;
        public float RightAnalogHorizontal;
        public Vector3 CameraWantedDirection;
        public float CameraLocalEulerAngleX;
        public bool GamepadModeActive;

        public uint SimulatedTick;

        public void ResetMoveInput() {
            FrameCount = 0;
            InputJump = false;
            InputFly = false;
            InputSink = false;
            InputStrafe = false;
            InputCrouch = false;
            InputHorizontal = 0;
            InputVertical = 0;
            InputTurn = 0;

            RightMouseButtonDown = false;
            RightMouseDragged = false;
            RightAnalogHorizontal = 0;
            CameraLocalEulerAngleX = 0;
            IntendedLocalDirection = Vector3.zero;
            IntendedWorldDirection = Vector3.zero;

            NormalizedMoveInput = Vector3.zero;
            TurnInput = new Vector3(InputTurn, 0, 0);
        }

        public bool HasAnyInput() {
            if (NormalizedMoveInput != Vector3.zero || TurnInput != Vector3.zero || InputJump != false) {
                return true;
            } else {
                return false;
            }
        }

        public bool HasWaterMoveInput() {
            if (NormalizedMoveInput != Vector3.zero || TurnInput != Vector3.zero || InputSink != false || InputFly != false) {
                return true;
            } else {
                return false;
            }
        }

        public bool HasFlyMoveInput() {
            if (NormalizedMoveInput != Vector3.zero || TurnInput != Vector3.zero || InputSink != false || InputFly != false) {
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
