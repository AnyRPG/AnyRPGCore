using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Animated Object Config", menuName = "AnyRPG/Interactable/AnimatedObjectConfig")]
    [System.Serializable]
    public class AnimatedObjectConfig : InteractableOptionConfig {

        [SerializeField]
        private float movementSpeed = 0.05f;

        [SerializeField]
        private float rotationSpeed = 10f;


        public InteractableOption GetInteractableOption(Interactable interactable) {
            return new AnimatedObject(interactable, this);
        }
    }

}