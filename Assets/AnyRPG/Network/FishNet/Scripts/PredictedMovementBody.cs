using UnityEngine;
using FishNet.Component.Prediction;
using FishNet.Object.Prediction;

namespace AnyRPG {
    public class PredictedMovementBody : IMovementBody {

        private PredictionRigidbody predictionRigidbody;

        public PredictedMovementBody(PredictionRigidbody predictionRigidbody) {
            this.predictionRigidbody = predictionRigidbody;
        }

        public void SetLinearVelocity(Vector3 velocity) {
            predictionRigidbody.Velocity(velocity);
        }

        public Vector3 GetLinearVelocity() {
            return predictionRigidbody.Rigidbody.linearVelocity;
        }

        public void AddForce(Vector3 force) {
            predictionRigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        public void SetPosition(Vector3 position) {
            predictionRigidbody.Rigidbody.position = position;

            // Force physics engine sync
            Physics.SyncTransforms();
        }

        public void SetRotation(Quaternion targetRotation) {
            predictionRigidbody.Rigidbody.rotation = targetRotation;

            //Physics.SyncTransforms();
        }

        public void AddExplosionForce(float explosionForce, Vector3 explosionCenter, float upwardModifier) {
            predictionRigidbody.AddExplosiveForce(explosionForce, explosionCenter, 0, upwardModifier, ForceMode.VelocityChange);
        }

        public Quaternion GetRotation() {
            return predictionRigidbody.Rigidbody.rotation;
        }

        public Vector3 GetPosition() {
            return predictionRigidbody.Rigidbody.position;
        }

        public Vector3 GetForward() {
            return predictionRigidbody.Rigidbody.rotation * Vector3.forward;
        }
    }
}