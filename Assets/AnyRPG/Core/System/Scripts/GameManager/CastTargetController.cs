using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class CastTargetController : ConfiguredMonoBehaviour {

        [Tooltip("A reference to the renderer that contains the material with the highlight circle")]
        [SerializeField]
        private MeshRenderer meshRenderer = null;

        [SerializeField]
        private Vector3 offset = Vector3.zero;

        private Color circleColor;

        private float circleRadius = 0f;

        public Color MyCircleColor { get => circleColor; set => circleColor = value; }

        // game manager references
        protected PlayerManager playerManager = null;
        protected CameraManager cameraManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            cameraManager = systemGameManager.CameraManager;
        }

        public void SetupController() {
            if (meshRenderer != null) {
                meshRenderer.material = new Material(systemConfigurationManager.DefaultCastingLightProjector);
            }

            circleColor = meshRenderer.material.color;
        }

        void Update() {
            //Debug.Log("CastTargettingController.Update()");
            FollowMouse();
        }

        private void SetOutOfRange(bool outOfRange) {
            //Debug.Log("CastTargettingController.HandleOutOfRange()");
            if (outOfRange == true) {
                if (meshRenderer.enabled) {
                    meshRenderer.enabled = false;
                }
            } else {
                if (!meshRenderer.enabled) {
                    meshRenderer.enabled = true;
                }
            }
        }

        private void FollowMouse() {
            //Debug.Log("CastTargettingController.FollowMouse()");
            if (playerManager.ActiveUnitController == null) {
                return;
            }
            if (!EventSystem.current.IsPointerOverGameObject()) {
                Ray ray = cameraManager.ActiveMainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                //if (Physics.Raycast(ray, out hit, 100)) {
                if (Physics.Raycast(ray, out hit, 100, playerManager.PlayerController.movementMask.value)) {
                    //Debug.Log("CastTargettingController.FollowMouse() hit movement mask at hit.point: " + hit.point + "; gameObject: " + hit.transform.gameObject.name + hit.transform.gameObject.layer);
                    Vector3 cameraPoint = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
                    if (Vector3.Distance(hit.point, playerManager.ActiveUnitController.transform.position) < 40f) {
                        //Debug.Log("CastTargettingController.FollowMouse() hit movement mask and was within 40 meters from player");
                        this.transform.position = cameraPoint;
                    }
                    SetOutOfRange(false);
                } else {
                    //Debug.Log("CastTargettingController.FollowMouse() did not hit movement mask: " + hit.transform.gameObject.name);
                    SetOutOfRange(true);
                }
            }
        }

        public void SetCircleColor(Color newColor) {
            //Debug.Log("CastTargettingController.SetCircleColor()");
            circleColor = newColor;
            meshRenderer.material.color = circleColor;
        }

        public void SetCircleRadius(float newRadius) {
            //Debug.Log("CastTargettingController.SetCircleRadius()");
            circleRadius = newRadius;
            transform.localScale = new Vector3(circleRadius * 2f, circleRadius * 2f, 1f);
        }


    }

}