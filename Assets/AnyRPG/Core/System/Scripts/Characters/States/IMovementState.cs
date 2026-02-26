using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IMovementState {
        void Enter(bool isReplay);

        void Update(bool isReplay);

        void Exit(bool isReplay);
    }
}