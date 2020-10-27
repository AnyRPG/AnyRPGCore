using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AnimatedObject : InteractableOption {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private AnimatedObjectConfig animatedObjectConfig = new AnimatedObjectConfig();

        [SerializeField]
        private float movementSpeed = 0.05f;

        [SerializeField]
        private float rotationSpeed = 10f;
        
        // by default it is considered closed when not using the sheathed position
        private bool objectOpen = false;

        // keep track of opening and closing
        private Coroutine coroutine = null;

        public AnimatedObject(Interactable interactable, AnimatedObjectConfig interactableConfig) : base(interactable) {
            this.animatedObjectConfig = interactableConfig;
            interactionPanelTitle = "Interactable";
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".AnimatedObject.Interact(" + (source == null ? "null" : source.name) +")");
            //if (coroutine != null) {
                //Debug.Log(gameObject.name + ".AnimatedObject.Interact(): coroutine is not null, exiting");
                //return false;
            //}
            base.Interact(source);
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();

            // loop through the animatedobjects prefabobjects
            // check their state (open / closed)
            if (interactable.MyPrefabProfile == null) {
                Debug.Log(gameObject.name + ".AnimatedObject.Interact(): prefabprofile was null");
                return false;
            }
            if (objectOpen) {
                coroutine = StartCoroutine(animateObject(interactable.MyPrefabProfile.SheathedRotation, interactable.MyPrefabProfile.SheathedPosition, interactable.MyPrefabProfile.SheathAudioProfile));
            } else {
                coroutine = StartCoroutine(animateObject(interactable.MyPrefabProfile.Rotation, interactable.MyPrefabProfile.Position, interactable.MyPrefabProfile.UnsheathAudioProfile));
            }
            // lerp them to the other state, using the values defined in their sheathed and regular positions

            return false;
        }

        private IEnumerator animateObject(Vector3 newAngle, Vector3 newPosition, AudioProfile audioProfile) {
            newAngle = new Vector3(newAngle.x < 0 ? newAngle.x + 360 : newAngle.x, newAngle.y < 0 ? newAngle.y + 360 : newAngle.y, newAngle.z < 0 ? newAngle.z + 360 : newAngle.z);
            Quaternion originalRotation = interactable.MySpawnReference.transform.localRotation;
            Vector3 originalPosition = interactable.MySpawnReference.transform.localPosition;
            //Debug.Log(gameObject.name + ".AnimatedObject.animateObject(" + newAngle + ", " + newPosition + "): original position: " + originalPosition + "; rotation: " + originalRotation);

            AudioSource audioSource = interactable.MySpawnReference.GetComponent<AudioSource>();
            if (audioSource != null && audioProfile != null && audioProfile.AudioClip != null) {
                //Debug.Log(gameObject.name + ".AnimatedObject.animateObject(): playing audioclip: " + audioProfile.AudioClip);
                audioSource.PlayOneShot(audioProfile.AudioClip);
            }

            // testing doing this first to allow an object to reverse before it's animation has completed
            objectOpen = !objectOpen;

            while (interactable.MySpawnReference.transform.localEulerAngles != newAngle || interactable.MySpawnReference.transform.localPosition != newPosition) {
                //Debug.Log(gameObject.name + ".AnimatedObject.animateObject(" + newAngle + ", " + newPosition + "): localEulerAngles: " + interactable.MySpawnReference.transform.localEulerAngles + "; position: " + interactable.MySpawnReference.transform.localPosition);
                //Quaternion newRotation = Quaternion.Lerp(originalRotation, Quaternion.Euler(newAngle), 0.01f);
                //Quaternion newRotation = Quaternion.RotateTowards(interactable.MySpawnReference.transform.localRotation, Quaternion.Euler(newAngle), rotationSpeed);

                // get a separate quaternion rotation to avoid issues with negative start angles
                //Quaternion tmpRotation = interactable.MySpawnReference.transform.localRotation * Quaternion.Euler(newAngle);
                //Vector3 realNewAngle = tmpRotation.eulerAngles;

                Quaternion newRotation = Quaternion.RotateTowards(Quaternion.Euler(interactable.MySpawnReference.transform.localEulerAngles), Quaternion.Euler(newAngle), rotationSpeed);
                //Quaternion newRotation = Quaternion.RotateTowards(interactable.MySpawnReference.transform.localRotation, Quaternion.Euler(realNewAngle), rotationSpeed);
                //Quaternion newRotation = Quaternion.RotateTowards(interactable.MySpawnReference.transform.localRotation, tmpRotation, rotationSpeed);
                Vector3 newLocation = Vector3.MoveTowards(interactable.MySpawnReference.transform.localPosition, newPosition, movementSpeed);
                interactable.MySpawnReference.transform.localPosition = newLocation;
                interactable.MySpawnReference.transform.localRotation = newRotation;
                yield return null;
            }
            //objectOpen = !objectOpen;
            //Debug.Log(gameObject.name + ".AnimatedObject.animateObject(" + newAngle + ", " + newPosition + "): localEulerAngles: " + interactable.MySpawnReference.transform.localEulerAngles + "; position: " + interactable.MySpawnReference.transform.localPosition + "; COMPLETE ANIMATION");
            coroutine = null;
        }

        /*
        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.MyInstance.AnimatedObjectWindow.CloseWindow();
        }
        */

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".AnimatedObject.HandldePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


        public override void OnDisable() {
            base.OnDisable();
            if (coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
    }

}