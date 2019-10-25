using AnyRPG;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AnyRPG {
    /// <summary>
    /// Custom character controller, to be used by attaching the component to an object
    /// and writing scripts attached to the same object that recieve the "StateUpdate" message
    /// </summary>
    public class AnyRPGCharacterController : MonoBehaviour {
        [SerializeField]
        Vector3 debugMove = Vector3.zero;

        [SerializeField]
        QueryTriggerInteraction triggerInteraction;

        [SerializeField]
        bool fixedTimeStep;

        [SerializeField]
        int fixedUpdatesPerSecond;

        public LayerMask Walkable;

        [SerializeField]
        Collider ownCollider;

        [SerializeField]
        public float radius = 0.5f;

        public float deltaTime { get; private set; }

        /// <summary>
        /// Total height of the controller from the bottom of the feet to the top of the head
        /// </summary>
        public Vector3 up { get { return transform.up; } }
        public Vector3 down { get { return -transform.up; } }
        public Transform currentlyClampedTo { get; set; }
        public float heightScale { get; set; }
        public float radiusScale { get; set; }
        public bool manualUpdateOnly { get; set; }

        public delegate void UpdateDelegate();
        public event UpdateDelegate AfterSingleUpdate;

        private Vector3 initialPosition;
        private Vector3 groundOffset;
        private Vector3 lastGroundPosition;
        private bool clamping = true;
        private bool slopeLimiting = true;

        private const float Tolerance = 0.05f;
        private const float TinyTolerance = 0.01f;
        private const string TemporaryLayer = "TempCast";
        private const int MaxPushbackIterations = 2;
        private int TemporaryLayerIndex;
        private float fixedDeltaTime;

        void Awake() {

            TemporaryLayerIndex = LayerMask.NameToLayer(TemporaryLayer);

            fixedDeltaTime = 1.0f / fixedUpdatesPerSecond;

            heightScale = 1.0f;

            manualUpdateOnly = false;

            gameObject.SendMessage("StateStart", SendMessageOptions.DontRequireReceiver);
        }

        void Update() {
            // If we are using a fixed timestep, ensure we run the main update loop
            // a sufficient number of times based on the Time.deltaTime
            if (manualUpdateOnly)
                return;

            if (!fixedTimeStep) {
                deltaTime = Time.deltaTime;

                SingleUpdate();
                return;
            } else {
                float delta = Time.deltaTime;

                while (delta > fixedDeltaTime) {
                    deltaTime = fixedDeltaTime;

                    SingleUpdate();

                    delta -= fixedDeltaTime;
                }

                if (delta > 0f) {
                    deltaTime = delta;

                    SingleUpdate();
                }
            }
        }

        public void ManualUpdate(float deltaTime) {
            this.deltaTime = deltaTime;

            SingleUpdate();
        }

        void SingleUpdate() {

            gameObject.SendMessage("StateUpdate", SendMessageOptions.DontRequireReceiver);

            if (AfterSingleUpdate != null)
                AfterSingleUpdate();
        }

    }


}