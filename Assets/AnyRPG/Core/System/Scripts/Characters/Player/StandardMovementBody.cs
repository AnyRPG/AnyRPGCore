using UnityEngine;

namespace AnyRPG {
    public class StandardMovementBody : IMovementBody {
        
        private Rigidbody rigidBody;

        public StandardMovementBody(Rigidbody rigidBody) {
            this.rigidBody = rigidBody;
        }

        public void SetLinearVelocity(Vector3 velocity) {
            //Debug.Log($"StandardMovementBody.SetLinearVelocity({velocity})");

            rigidBody.linearVelocity = velocity;
        }

        public Vector3 GetLinearVelocity() {
            return rigidBody.linearVelocity;
        }

        public void AddForce(Vector3 force) {
            //Debug.Log($"StandardMovementBody.AddForce({force})");

            rigidBody.AddRelativeForce(force, ForceMode.VelocityChange);
        }

        public void SetPosition(Vector3 position) {
            //Debug.Log($"StandardMovementBody.SetPosition({position})");

            rigidBody.position = position;

            Physics.SyncTransforms();
        }

        public void SetRotation(Quaternion targetRotation) {
            //Debug.Log($"StandardMovementBody.SetRotation({targetRotation})");

            rigidBody.rotation = targetRotation;
            //rigidBody.MoveRotation(targetRotation);

            //Physics.SyncTransforms();
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