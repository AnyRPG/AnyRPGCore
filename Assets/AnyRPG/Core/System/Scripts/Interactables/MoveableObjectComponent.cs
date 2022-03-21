using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MoveableObjectComponent : InteractableOptionComponent {

        public MoveableObjectProps Props { get => interactableOptionProps as MoveableObjectProps; }

        // by default it is considered closed
        private bool objectOpen = false;

        // track state of looping
        private bool looping = false;

        // keep track of opening and closing
        private Coroutine moveCoroutine = null;

        // keep track of looping
        private Coroutine loopCoroutine = null;

        private Vector3 originalPosition = Vector3.zero;
        private Vector3 originalRotation = Vector3.zero;
        AudioSource audioSource = null;

        public MoveableObjectComponent(Interactable interactable, MoveableObjectProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactableOptionProps.InteractionPanelTitle = "Interactable";
            if (Props.MoveableObject != null) {
                originalPosition = Props.MoveableObject.transform.localPosition;
                originalRotation = Props.MoveableObject.transform.localEulerAngles;
                audioSource = Props.MoveableObject.GetComponent<AudioSource>();
            }
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

        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false, float factionValue = 0, bool processNonCombatCheck = true) {

            if (Props.SwitchOnly == true) {
                return false;
            }
            return base.CanInteract(processRangeCheck, passedRangeCheck, factionValue, processNonCombatCheck);
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".AnimatedObject.Interact(" + (source == null ? "null" : source.name) +")");
            //if (coroutine != null) {
                //Debug.Log(gameObject.name + ".AnimatedObject.Interact(): coroutine is not null, exiting");
                //return false;
            //}
            base.Interact(source, optionIndex);
            uIManager.interactionWindow.CloseWindow();

            // loop through the animatedobjects prefabobjects
            // check their state (open / closed)
            if (Props.MoveableObject == null) {
                Debug.Log("MoveableObject.Interact(): gameObject was null. Check Inspector");
                return false;
            }
            if (Props.Loop == true) {
                if (looping == true) {
                    looping = false;
                } else {
                    looping = true;
                    ChooseMovement();
                    loopCoroutine = interactable.StartCoroutine(LoopAnimation());
                }
            } else {
                ChooseMovement();
            }
            // lerp them to the other state, using the values defined in their sheathed and regular positions

            return false;
        }

        private void ChooseMovement() {
            //Debug.Log("MoveableObjectComponent.ChooseMovement()");

            if (objectOpen == true) {
                moveCoroutine = interactable.StartCoroutine(AnimateObject(originalRotation, originalPosition, Props.CloseAudioClip));
            } else {
                moveCoroutine = interactable.StartCoroutine(AnimateObject(Props.TargetRotation, Props.TargetPosition, Props.OpenAudioClip));
            }
        }

        private IEnumerator LoopAnimation() {
            //Debug.Log("MoveableObjectComponent.LoopAnimation()");

            yield return null;
            while (looping == true) {
                if (moveCoroutine == null) {

                    // allow delay
                    if (Props.DelayTime > 0f) {
                        yield return new WaitForSeconds(Props.DelayTime);
                    }

                    // check to ensure loop hasn't been deactivated during loop wait
                    if (moveCoroutine == null && looping == true) {
                        ChooseMovement();
                    }
                }
                yield return null;
            }

            // looping could be turned off in the middle of a movement so give a chance to clean it up
            if (moveCoroutine != null) {
                interactable.StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

        }

        private IEnumerator AnimateObject(Vector3 newAngle, Vector3 newPosition, AudioClip audioClip) {
            //Debug.Log("MoveableObjectComponent.AnimateObject(" + newAngle + ", " + newPosition + ")");

            newAngle = new Vector3(newAngle.x < 0 ? newAngle.x + 360 : newAngle.x, newAngle.y < 0 ? newAngle.y + 360 : newAngle.y, newAngle.z < 0 ? newAngle.z + 360 : newAngle.z);
            //Quaternion originalRotation = interactable.SpawnReference.transform.localRotation;
            //Vector3 originalPosition = interactable.SpawnReference.transform.localPosition;
            //Debug.Log(gameObject.name + ".AnimatedObject.animateObject(" + newAngle + ", " + newPosition + "): original position: " + originalPosition + "; rotation: " + originalRotation);

            if (audioSource != null && audioClip != null) {
                //Debug.Log(gameObject.name + ".AnimatedObject.animateObject(): playing audioclip: " + audioProfile.AudioClip);
                audioSource.PlayOneShot(audioClip);
            }

            // testing doing this first to allow an object to reverse before its animation has completed
            objectOpen = !objectOpen;

            while (Props.MoveableObject.transform.localEulerAngles != newAngle || Props.MoveableObject.transform.localPosition != newPosition) {
                //Debug.Log(gameObject.name + ".AnimatedObject.animateObject(" + newAngle + ", " + newPosition + "): localEulerAngles: " + interactable.MySpawnReference.transform.localEulerAngles + "; position: " + interactable.MySpawnReference.transform.localPosition);
                //Quaternion newRotation = Quaternion.Lerp(originalRotation, Quaternion.Euler(newAngle), 0.01f);
                //Quaternion newRotation = Quaternion.RotateTowards(interactable.MySpawnReference.transform.localRotation, Quaternion.Euler(newAngle), rotationSpeed);

                // get a separate quaternion rotation to avoid issues with negative start angles
                //Quaternion tmpRotation = interactable.MySpawnReference.transform.localRotation * Quaternion.Euler(newAngle);
                //Vector3 realNewAngle = tmpRotation.eulerAngles;

                Quaternion newRotation = Quaternion.RotateTowards(Quaternion.Euler(Props.MoveableObject.transform.localEulerAngles), Quaternion.Euler(newAngle), Props.RotationSpeed * Time.deltaTime);
                //Quaternion newRotation = Quaternion.RotateTowards(interactable.MySpawnReference.transform.localRotation, Quaternion.Euler(realNewAngle), rotationSpeed);
                //Quaternion newRotation = Quaternion.RotateTowards(interactable.MySpawnReference.transform.localRotation, tmpRotation, rotationSpeed);
                Vector3 newLocation = Vector3.MoveTowards(Props.MoveableObject.transform.localPosition, newPosition, Props.MovementSpeed * Time.deltaTime);
                Props.MoveableObject.transform.localPosition = newLocation;
                Props.MoveableObject.transform.localRotation = newRotation;
                yield return null;
            }
            //objectOpen = !objectOpen;
            //Debug.Log(gameObject.name + ".AnimatedObject.animateObject(" + newAngle + ", " + newPosition + "): localEulerAngles: " + interactable.MySpawnReference.transform.localEulerAngles + "; position: " + interactable.MySpawnReference.transform.localPosition + "; COMPLETE ANIMATION");
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