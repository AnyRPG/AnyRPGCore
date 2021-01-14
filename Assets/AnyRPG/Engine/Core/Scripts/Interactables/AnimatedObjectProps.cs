using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class AnimatedObjectProps : InteractableOptionProps {

        [SerializeField]
        private float movementSpeed = 0.05f;

        [SerializeField]
        private float rotationSpeed = 10f;

        public float MovementSpeed { get => movementSpeed; set => movementSpeed = value; }
        public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new AnimatedObjectComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }
    }

}