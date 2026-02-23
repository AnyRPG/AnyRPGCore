using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AnyRPG {
    public class StandardMovementBody : IMovementBody {
        
        private Rigidbody rigidBody;

        public StandardMovementBody(Rigidbody rigidBody) {
            this.rigidBody = rigidBody;
        }

        public void SetLinearVelocity(Vector3 velocity) {
            rigidBody.linearVelocity = velocity;
        }

        public Vector3 GetLinearVelocity() {
            return rigidBody.linearVelocity;
        }

        public void AddForce(Vector3 force) {
            rigidBody.AddRelativeForce(force, ForceMode.VelocityChange);
        }

        public void SetPosition(Vector3 position) {
            rigidBody.position = position;

            Physics.SyncTransforms();
        }

        public void SetRotation(Quaternion targetRotation) {
            rigidBody.rotation = targetRotation;
        }

        public void AddExplosionForce(float explosionForce, Vector3 explosionCenter, float upwardModifier) {
            rigidBody.AddExplosionForce(explosionForce, explosionCenter, 0, upwardModifier, ForceMode.VelocityChange);
        }

        public Quaternion GetRotation() {
            return rigidBody.rotation;
        }

        public Vector3 GetPosition() {
            return rigidBody.position;
        }
    }
}