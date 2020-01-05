using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AnimatedObject : InteractableOption {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        // by default it is considered closed when not using the sheathed position
        private bool objectOpen = false;

        // keep track of opening and closing
        private Coroutine coroutine = null;

        /*
        public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyAnimatedObjectInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyAnimatedObjectInteractionPanelImage : base.MyIcon); }
        public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyAnimatedObjectNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyAnimatedObjectNamePlateImage : base.MyNamePlateImage); }
        */

        protected override void Start() {
            base.Start();
            interactionPanelTitle = "Interactable";

        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".AnimatedObject.Interact(" + (source == null ? "null" : source.name) +")");
            if (coroutine != null) {
                //Debug.Log(gameObject.name + ".AnimatedObject.Interact(): coroutine is not null, exiting");
                return false;
            }
            base.Interact(source);
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();

            // loop through the animatedobjects prefabobjects
            // check their state (open / closed)
            if (interactable.MyPrefabProfile == null) {
                //Debug.Log(gameObject.name + ".AnimatedObject.Interact(): prefabprofile was null");
                return false;
            }
            if (objectOpen) {
                coroutine = StartCoroutine(animateObject(interactable.MyPrefabProfile.MyRotation));
            } else {
                coroutine = StartCoroutine(animateObject(interactable.MyPrefabProfile.MySheathedRotation));
            }
            // lerp them to the other state, using the values defined in their sheathed and regular positions

            return false;
        }

        private IEnumerator animateObject(Vector3 newAngle) {
            //Debug.Log(gameObject.name + ".AnimatedObject.animateObject(" + newAngle + ")");
            Quaternion originalRotation = interactable.MySpawnReference.transform.localRotation;
            while (!(interactable.MySpawnReference.transform.localEulerAngles == newAngle)) {
                //Debug.Log(gameObject.name + ".AnimatedObject.animateObject(" + newAngle + "): localEulerAngles: " + interactable.MySpawnReference.transform.localEulerAngles);
                //Quaternion newRotation = Quaternion.Lerp(originalRotation, Quaternion.Euler(newAngle), 0.01f);
                Quaternion newRotation = Quaternion.RotateTowards(interactable.MySpawnReference.transform.localRotation, Quaternion.Euler(newAngle), 10f);
                interactable.MySpawnReference.transform.localRotation = newRotation;
                yield return null;
            }
            objectOpen = !objectOpen;
            //Debug.Log(gameObject.name + ".AnimatedObject.animateObject(" + newAngle + "): done rotation");
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

        public override void OnDisable() {
            base.OnDisable();
            if (coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
    }

}