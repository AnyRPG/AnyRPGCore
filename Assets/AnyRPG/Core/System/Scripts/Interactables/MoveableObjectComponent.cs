using AnyRPG;
using System;
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
                
                // all angles are rounded to 4 decimals and than made positive to attempt to avoid rotations greater than 180 degrees
                originalRotation = GetTranslatedEulerAngles(Props.MoveableObject.transform.localEulerAngles);
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
            //Debug.Log($"{gameObject.name}.AnimatedObject.Interact(" + (source == null ? "null" : source.name) +")");

            base.Interact(source, optionIndex);
            uIManager.interactionWindow.CloseWindow();

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
            //Debug.Log($"MoveableObjectComponent.AnimateObject({newAngle.x} {newAngle.y} {newAngle.z}, {newPosition}) localEulerAngles: {Props.MoveableObject.transform.localEulerAngles.x}, {Props.MoveableObject.transform.localEulerAngles.y}, {Props.MoveableObject.transform.localEulerAngles.z}; position: {Props.MoveableObject.transform.localPosition}; objectopen = {objectOpen}");

            newAngle = GetTranslatedEulerAngles(newAngle);

            if (audioSource != null && audioClip != null) {
                //Debug.Log($"{gameObject.name}.AnimatedObject.animateObject(): playing audioclip: " + audioProfile.AudioClip);
                audioSource.PlayOneShot(audioClip);
            }

            // setting open / closed state first to allow an object to reverse before its animation has completed
            objectOpen = !objectOpen;

            while (Props.MoveableObject.transform.localEulerAngles != newAngle || Props.MoveableObject.transform.localPosition != newPosition) {
                //Debug.Log(Props.MoveableObject.name + ".AnimatedObject.animateObject(" + newAngle + ", " + newPosition + "): localEulerAngles: " + GetTranslatedEulerAngles(Props.MoveableObject.transform.localEulerAngles) + "; position: " + Props.MoveableObject.transform.localPosition);

                Quaternion newRotation = Quaternion.RotateTowards(Quaternion.Euler(Props.MoveableObject.transform.localEulerAngles), Quaternion.Euler(newAngle), Props.RotationSpeed * Time.deltaTime);
                Vector3 newLocation = Vector3.MoveTowards(Props.MoveableObject.transform.localPosition, newPosition, Props.MovementSpeed * Time.deltaTime);

                Props.MoveableObject.transform.localPosition = newLocation;
                Props.MoveableObject.transform.localRotation = newRotation;

                yield return null;
            }

            //Debug.Log(Props.MoveableObject.name + ".AnimatedObject.animateObject(" + newAngle + ", " + newPosition + "): localEulerAngles: " + Props.MoveableObject.transform.localEulerAngles + "; position: " + Props.MoveableObject.transform.localPosition + "; COMPLETE ANIMATION");
            moveCoroutine = null;
        }

        private Vector3 GetTranslatedEulerAngles(Vector3 originalAngle) {
            // here all values are rounded to 4 decimals to avoid floating point accuracy issues that occur when adding whole numbers greater than 9
            // eg : 3 + 0.123456 = 3.123456
            // eg : 36 + 0.123456 = 36.12346
            // eg : 360 + 0.123456 = 360.1234
            return new Vector3(MathF.Round(originalAngle.x, 4) < 0f ? MathF.Round(originalAngle.x, 4) + 360f : MathF.Round(originalAngle.x, 4), MathF.Round(originalAngle.y, 4) < 0f ? MathF.Round(originalAngle.y, 4) + 360f : MathF.Round(originalAngle.y, 4), MathF.Round(originalAngle.z, 4) < 0f ? MathF.Round(originalAngle.z, 4) + 360f : MathF.Round(originalAngle.z, 4));
        }

        /*
        public override void StopInteract() {
            base.StopInteract();
            SystemGameManager.Instance.UIManager.AnimatedObjectWindow.CloseWindow();
        }
        */

    }

}