using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class MoveableObjectProps : InteractableOptionProps {

        [Header("Moveable Object")]

        [Tooltip("If true, this option can only be interacted with via a switch.")]
        [SerializeField]
        protected bool switchOnly = false;

        [Tooltip("A reference to the gameObject in the scene that will be moved.")]
        [SerializeField]
        protected GameObject moveableObject = null;

        [Header("Options")]

        [Tooltip("Meters Per Second")]
        [SerializeField]
        private float movementSpeed = 1f;

        [Tooltip("Degrees Per Second")]
        [SerializeField]
        private float rotationSpeed = 45f;

        [Tooltip("Amount of time to delay inbetween open and close actions when looping")]
        [SerializeField]
        private float delayTime = 1f;

        [Tooltip("If true, the animation will keep looping while the switch is in the on state")]
        [SerializeField]
        private bool loop = false;

        [Header("Position")]

        [Tooltip("The target positon when opened")]
        [SerializeField]
        private Vector3 targetPosition = Vector3.zero;

        /*
        [Tooltip("The target rotation when opened")]
        [SerializeField]
        private bool rotationIsGlobal = false;
        */

        [Tooltip("The target rotation when opened")]
        [SerializeField]
        private Vector3 targetRotation = Vector3.zero;

        [Header("Audio")]

        [Tooltip("Audio clip to play when opening")]
        [SerializeField]
        private AudioClip openAudioClip = null;

        [Tooltip("Audio clip to play when closing")]
        [SerializeField]
        private AudioClip closeAudioClip = null;


        public float MovementSpeed { get => movementSpeed; set => movementSpeed = value; }
        public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }
        public bool Loop { get => loop; set => loop = value; }
        public GameObject MoveableObject { get => moveableObject; set => moveableObject = value; }
        public Vector3 TargetPosition { get => targetPosition; set => targetPosition = value; }
        public Vector3 TargetRotation { get => targetRotation; set => targetRotation = value; }
        public bool SwitchOnly { get => switchOnly; set => switchOnly = value; }
        public AudioClip OpenAudioClip { get => openAudioClip; set => openAudioClip = value; }
        public AudioClip CloseAudioClip { get => closeAudioClip; set => closeAudioClip = value; }
        public float DelayTime { get => delayTime; set => delayTime = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new MoveableObjectComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }
    }

}