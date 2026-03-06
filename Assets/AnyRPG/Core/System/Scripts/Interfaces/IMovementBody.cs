using UnityEngine;

namespace AnyRPG {
    public interface IMovementBody {
        void SetLinearVelocity(Vector3 velocity);
        void AddForce(Vector3 force);
        void SetPosition(Vector3 position);
        Vector3 GetLinearVelocity();
        void SetRotation(Quaternion targetRotation);
        void AddExplosionForce(float explosionForce, Vector3 explosionCenter, float upwardModifier);
        Quaternion GetRotation();
        Vector3 GetForward();
        Vector3 GetPosition();
    }

}