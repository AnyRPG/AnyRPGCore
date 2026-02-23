using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IMovementState {
        void Enter();

        void Update();

        void Exit();
    }
}