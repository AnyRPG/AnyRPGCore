using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script helps rotation constraints work properly by resetting rotation inertia settings, which can cause rotation constraints to not work properly

namespace AnyRPG {
    public class RotationFixer : MonoBehaviour {

        void Start() {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.centerOfMass = Vector3.zero;
            rb.inertiaTensorRotation = Quaternion.identity;
        }

    }

}
