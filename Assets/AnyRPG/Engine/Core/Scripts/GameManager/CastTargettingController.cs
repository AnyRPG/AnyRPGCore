using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class CastTargettingController : MonoBehaviour {

        [SerializeField]
        private Projector castTargettingProjector = null;

        [SerializeField]
        private Vector3 offset = Vector3.zero;

        private Color circleColor;

        private float circleRadius = 0f;

        public Color MyCircleColor { get => circleColor; set => circleColor = value; }

        void Start() {
        }

        public void SetupController() {
            if (castTargettingProjector != null) {
                castTargettingProjector.material = SystemConfigurationManager.MyInstance.MyDefaultCastingLightProjector;
            }

            circleColor = castTargettingProjector.material.color;
        }

        void Update() {
            //Debug.Log("CastTargettingController.Update()");
            FollowMouse();
        }

        private void SetOutOfRange(bool outOfRange) {
            //Debug.Log("CastTargettingController.HandleOutOfRange()");
            if (outOfRange == true) {
                if (castTargettingProjector.enabled) {
                    castTargettingProjector.enabled = false;
                }
            } else {
                if (!castTargettingProjector.enabled) {
                    castTargettingProjector.enabled = true;
                }
            }
        }

        private void FollowMouse() {
            //Debug.Log("CastTargettingController.FollowMouse()");
            if (PlayerManager.MyInstance == null || PlayerManager.MyInstance.ActiveUnitController == null) {
                return;
            }
            if (!EventSystem.current.IsPointerOverGameObject()) {
                Ray ray = CameraManager.MyInstance.MyActiveMainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                //if (Physics.Raycast(ray, out hit, 100)) {
                if (Physics.Raycast(ray, out hit, 100, PlayerManager.MyInstance.PlayerController.movementMask.value)) {
                    //Debug.Log("CastTargettingController.FollowMouse() hit movement mask at hit.point: " + hit.point + "; gameObject: " + hit.transform.gameObject.name + hit.transform.gameObject.layer);
                    Vector3 cameraPoint = new Vector3(hit.point.x, hit.point.y + 4, hit.point.z);
                    if (Vector3.Distance(hit.point, PlayerManager.MyInstance.ActiveUnitController.transform.position) < 40f) {
                        //Debug.Log("CastTargettingController.FollowMouse() hit movement mask and was within 40 meters from player");
                        this.transform.position = cameraPoint;
                    }
                } else {
                    //Debug.Log("CastTargettingController.FollowMouse() did not hit movement mask: " + hit.transform.gameObject.name);
                    SetOutOfRange(true);
                }
            }
        }

        public void SetCircleColor(Color newColor) {
            //Debug.Log("CastTargettingController.SetCircleColor()");
            circleColor = newColor;
            castTargettingProjector.material.color = circleColor;
        }

        public void SetCircleRadius(float newRadius) {
            //Debug.Log("CastTargettingController.SetCircleRadius()");
            circleRadius = newRadius;
            castTargettingProjector.orthographicSize = circleRadius;
        }


    }

}