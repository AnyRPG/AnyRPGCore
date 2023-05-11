using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class OutlineController : ConfiguredClass {

        private Interactable interactable;
        private Color outlineColor;

        // state tracking
        protected bool outlineQueued = false;
        protected bool isOutlined = false;

        // game manager references
        protected CameraManager cameraManager = null;

        public OutlineController(Interactable interactable, SystemGameManager systemGameManager) {
            this.interactable = interactable;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            cameraManager = systemGameManager.CameraManager;
        }

        public void Update() {
            if (outlineQueued == false) {
                return;
            }

            if (interactable.IsBuilding() == true) {
                return;
            }

            interactable.StartCoroutine(OutlineNextFrameDelay());
        }

        public IEnumerator OutlineNextFrameDelay() {
            yield return null;
            OutlineNextFrame();
        }

        private void OutlineNextFrame() {
            if (outlineQueued == false) {
                return;
            }

            if (interactable.IsBuilding() == false) {
                ActivateOutline();
            }
        }

        protected virtual void ActivateOutline() {
            //Debug.Log($"{gameObject.name}.Interactable.OnMouseEnter(): hasMeshRenderer && glowOnMouseOver == true");

            if (isOutlined == false) {
                isOutlined = true;
                SendRequestToCameraHighlighter();
            }
            outlineQueued = false;
        }

        public void SendRequestToCameraHighlighter() {
            //Debug.Log($"{interactable.gameObject.name}.OutlineController.SendRequestToCameraHighlighter()");

            outlineColor = interactable.GetGlowColor();

            if (interactable.ObjectMaterialController.MeshRenderers != null && interactable.ObjectMaterialController.MeshRenderers.Length > 0) {
                cameraManager.MainCameraHighlighter.AddOutlinedObject(interactable, outlineColor, interactable.ObjectMaterialController.MeshRenderers);
            }/* else {
                // this can happen in the case where the interactable is made entirely out of particle effects
                //Debug.Log($"{interactable.gameObject.name}.OutlineController.SendRequestToCameraHighlighter() no mesh renderers found");
            }*/
        }

        public void TurnOnOutline() {
            //Debug.Log($"{interactable.gameObject.name}.OutlineController.TurnOnOutline()");

            if (interactable.IsBuilding() == false) {
                ActivateOutline();
            } else {
                outlineQueued = true;
            }
        }

        public void TurnOffOutline() {
            //Debug.Log($"{interactable.gameObject.name}.OutlineController.TurnOffOutline()");

            if (isOutlined == false) {
                // there was nothing to interact with on mouseover so just exit instead of trying to reset materials
                outlineQueued = false;
                return;
            }
            RevertMaterialChange();
        }

        private void RevertOutline() {
            //Debug.Log($"{interactable.gameObject.name}.Interactable.RevertOutline()");

            cameraManager.MainCameraHighlighter.RemoveOutlinedObject(interactable);
        }

        private void RevertMaterialChange() {
            //Debug.Log($"{interactable.gameObject.name}.Interactable.RevertMaterialChange()");

            if (isOutlined == false) {
                return;
            }

            isOutlined = false;

            RevertOutline();
        }
    }

}
