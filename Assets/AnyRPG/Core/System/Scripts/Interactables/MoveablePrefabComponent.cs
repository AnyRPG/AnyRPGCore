using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MoveablePrefabComponent : InteractableOptionComponent {

        public MoveablePrefabProps Props { get => interactableOptionProps as MoveablePrefabProps; }

        // by default it is considered closed when not using the sheathed position
        private bool objectOpen = false;

        // track state of looping
        private bool looping = false;

        // keep track of opening and closing
        private Coroutine moveCoroutine = null;

        // keep track of looping
        private Coroutine loopCoroutine = null;

        public MoveablePrefabComponent(Interactable interactable, MoveablePrefabProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactableOptionProps.InteractionPanelTitle = "Interactable";
        }

        public override void Cleanup() {
            base.Cleanup();
            if (moveCoroutine != null) {
                interactable.StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
            if (loopCoroutine != null) {
                interactable.StopCoroutine(loopCoroutine);
                loopCoroutine = null;
            }
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log($"{gameObject.name}.AnimatedObject.Interact(" + (source == null ? "null" : source.name) +")");
            //if (coroutine != null) {
                //Debug.Log($"{gameObject.name}.AnimatedObject.Interact(): coroutine is not null, exiting");
                //return false;
            //}
            base.Interact(source, optionIndex);
            uIManager.interactionWindow.CloseWindow();

            // loop through the animatedobjects prefabobjects
            // check their state (open / closed)
            if (interactable.PrefabProfile == null) {
                Debug.Log("AnimatedObject.Interact(): prefabprofile was null");
                return false;
            }
            if (Props.Loop == true) {
                if (looping == true) {
                    looping = false;
                } else {
                    looping = true;
                    loopCoroutine = interactable.StartCoroutine(LoopAnimation());
                }
            } else {
                ChooseMovement();
            }
            // lerp them to the other state, using the values defined in their sheathed and regular positions

            return false;
        }

        private void ChooseMovement() {
            if (objectOpen) {
                moveCoroutine = interactable.StartCoroutine(animateObject(interactable.PrefabProfile.SheathedRotation, interactable.PrefabProfile.SheathedPosition, interactable.PrefabProfile.SheathAudioProfile));
            } else {
                moveCoroutine = interactable.StartCoroutine(animateObject(interactable.PrefabProfile.Rotation, interactable.PrefabProfile.Position, interactable.PrefabProfile.UnsheathAudioProfile));
            }
        }

        private IEnumerator LoopAnimation() {
            while (looping == true) {
                if (moveCoroutine == null) {
                    ChooseMovement();
                }
                yield return null;
            }

            // looping could be turned off in the middle of a movement so give a chance to clean it up
            if (moveCoroutine != null) {
                interactable.StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

        }

        private IEnumerator animateObject(Vector3 newAngle, Vector3 newPosition, AudioProfile audioProfile) {
            newAngle = new Vector3(newAngle.x < 0 ? newAngle.x + 360 : newAngle.x, newAngle.y < 0 ? newAngle.y + 360 : newAngle.y, newAngle.z < 0 ? newAngle.z + 360 : newAngle.z);
            Quaternion originalRotation = interactable.SpawnReference.transform.localRotation;
            Vector3 originalPosition = interactable.SpawnReference.transform.localPosition;
            //Debug.Log($"{gameObject.name}.AnimatedObject.animateObject(" + newAngle + ", " + newPosition + "): original position: " + originalPosition + "; rotation: " + originalRotation);

            AudioSource audioSource = interactable.SpawnReference.GetComponent<AudioSource>();
            if (audioSource != null && audioProfile != null && audioProfile.AudioClip != null) {
                //Debug.Log($"{gameObject.name}.AnimatedObject.animateObject(): playing audioclip: " + audioProfile.AudioClip);
                audioSource.PlayOneShot(audioProfile.AudioClip);
            }

            // testing doing this first to allow an object to reverse before it's animation has completed
            objectOpen = !objectOpen;

            while (interactable.SpawnReference.transform.localEulerAngles != newAngle || interactable.SpawnReference.transform.localPosition != newPosition) {
                //Debug.Log($"{gameObject.name}.AnimatedObject.animateObject(" + newAngle + ", " + newPosition + "): localEulerAngles: " + interactable.MySpawnReference.transform.localEulerAngles + "; position: " + interactable.MySpawnReference.transform.localPosition);
                //Quaternion newRotation = Quaternion.Lerp(originalRotation, Quaternion.Euler(newAngle), 0.01f);
                //Quaternion newRotation = Quaternion.RotateTowards(interactable.MySpawnReference.transform.localRotation, Quaternion.Euler(newAngle), rotationSpeed);

                // get a separate quaternion rotation to avoid issues with negative start angles
                //Quaternion tmpRotation = interactable.MySpawnReference.transform.localRotation * Quaternion.Euler(newAngle);
                //Vector3 realNewAngle = tmpRotation.eulerAngles;

                Quaternion newRotation = Quaternion.RotateTowards(Quaternion.Euler(interactable.SpawnReference.transform.localEulerAngles), Quaternion.Euler(newAngle), Props.RotationSpeed);
                //Quaternion newRotation = Quaternion.RotateTowards(interactable.MySpawnReference.transform.localRotation, Quaternion.Euler(realNewAngle), rotationSpeed);
                //Quaternion newRotation = Quaternion.RotateTowards(interactable.MySpawnReference.transform.localRotation, tmpRotation, rotationSpeed);
                Vector3 newLocation = Vector3.MoveTowards(interactable.SpawnReference.transform.localPosition, newPosition, Props.MovementSpeed);
                interactable.SpawnReference.transform.localPosition = newLocation;
                interactable.SpawnReference.transform.localRotation = newRotation;
                yield return null;
            }
            //objectOpen = !objectOpen;
            //Debug.Log($"{gameObject.name}.AnimatedObject.animateObject(" + newAngle + ", " + newPosition + "): localEulerAngles: " + interactable.MySpawnReference.transform.localEulerAngles + "; position: " + interactable.MySpawnReference.transform.localPosition + "; COMPLETE ANIMATION");
            moveCoroutine = null;
        }

        /*
        public override void StopInteract() {
            base.StopInteract();
            SystemGameManager.Instance.UIManager.AnimatedObjectWindow.CloseWindow();
        }
        */

    }

}