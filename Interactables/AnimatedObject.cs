using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AnimatedObject : InteractableOption {

        [SerializeField]
        private AnimatedObjectProps animatedObjectProps = new AnimatedObjectProps();

        [SerializeField]
        private float movementSpeed = 0.05f;

        [SerializeField]
        private float rotationSpeed = 10f;

        public override InteractableOptionProps InteractableOptionProps { get => animatedObjectProps; }
    }

}