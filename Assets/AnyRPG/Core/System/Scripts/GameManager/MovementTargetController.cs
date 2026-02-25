using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace AnyRPG {
    public class MovementTargetController : ConfiguredMonoBehaviour {

        [Tooltip("A reference to the renderer that contains the material with the highlight circle")]
        [SerializeField]
        private MeshRenderer meshRenderer = null;

        [Tooltip("A reference to the decal projector that will project the graphic on the ground")]
        [SerializeField]
        private DecalProjector decalProjector = null;

        private Color circleColor;

        private float circleRadius = 0f;

        private static readonly int CLICK_TIME_PROPERTY = Shader.PropertyToID("_ClickTime");

        public Color CircleColor { get => circleColor; set => circleColor = value; }

        // game manager references
        protected PlayerManagerClient playerManagerClient = null;
        protected ControlsManager controlsManager = null;
        protected LevelManagerClient levelManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            meshRenderer.enabled = false;
            DisableProjector();
            levelManagerClient.OnLevelUnload += HandleLevelUnloadClient;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
            controlsManager = systemGameManager.ControlsManager;
            levelManagerClient = systemGameManager.LevelManagerClient;
        }

        private void HandleLevelUnloadClient(int sceneHandle, string sceneName) {
            DisableProjector();
        }

        public void SetupController() {
            if (meshRenderer != null) {
                meshRenderer.material = new Material(systemConfigurationManager.DefaultCastingLightProjector);
            }
            if (decalProjector != null) {
                decalProjector.material = new Material(systemConfigurationManager.DefaultMovementTargetCircle);
            }

            circleColor = meshRenderer.material.color;
        }

        public void SetCircleColor(Color newColor) {
            //Debug.Log("MovementTargetController.SetCircleColor()");
            circleColor = newColor;
            meshRenderer.material.color = circleColor;
        }

        public void SetCircleRadius(float newRadius) {
            //Debug.Log("MovementTargetController.SetCircleRadius()");
            circleRadius = newRadius;
            transform.localScale = new Vector3(circleRadius * 2f, circleRadius * 2f, 1f);
        }

        public void SetPosition(Vector3 point) {
            Vector3 finalPoint = new Vector3(point.x, point.y + 0.5f, point.z);
            transform.position = finalPoint;
            //meshRenderer.material.SetFloat(nameID:CLICK_TIME_PROPERTY, Time.time);
            decalProjector.material.SetFloat(nameID: CLICK_TIME_PROPERTY, Time.time);
            decalProjector.enabled = true;
        }

        public void DisableProjector() {
            //Debug.Log("MovementTargetController.DisableProjector()");

            if (decalProjector.enabled == false) {
                return;
            }
            decalProjector.enabled = false;
        }
    }

}