using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CameraShader : MonoBehaviour {

        [Header("Base layer")]

        [Tooltip("If true, the base layer will be rendered")]
        [SerializeField]
        private bool renderBase = true;

        [Tooltip("The base layers that will be rendered")]
        [SerializeField]
        private LayerMask miniMapMask = ~0;

        [Tooltip("The shader that will be used to render the layers on the minimap mask. Leave blank to render with the default shader.")]
        [SerializeField]
        private Shader miniMapShader = null;

        [Header("Overlay layer")]

        [Tooltip("If true, the overlay layer will be rendered")]
        [SerializeField]
        private bool renderOverlay = true;

        [Tooltip("The overlay layers that will be rendered")]
        [SerializeField]
        private LayerMask overlayMask = ~0;

        [Tooltip("The shader that will be used to render the layers on the overlay mask. Leave blank to render with the default shader.")]
        [SerializeField]
        private Shader overlayShader = null;

        [Header("Performance")]

        [Tooltip("If true, the minimap will update at a set frame rate, instead of every frame")]
        [SerializeField]
        private bool limitFrameRates = false;

        [Tooltip("If frame rates are limited, this is the number of times per second the minimap will refresh")]
        [SerializeField]
        private int targetFrameRate = 10;

        private float accumulatedTime = 0;
        private float frameLength = 0f;

        private Camera miniMapCamera;

        // Start is called before the first frame update
        void Start() {
            miniMapCamera = GetComponent<Camera>();
            miniMapCamera.enabled = false;

            if (limitFrameRates == true) {
                frameLength = 1f / targetFrameRate;
            }
        }

        // Update is called once per frame
        void Update() {
            if (limitFrameRates == false) {
                CaptureFrame();
            } else {
                accumulatedTime += Time.deltaTime;
                if (accumulatedTime >= frameLength) {
                    accumulatedTime -= frameLength;
                    CaptureFrame();
                }
            }
        }

        public void CaptureFrame() {
            if (renderBase == true) {
                miniMapCamera.cullingMask = miniMapMask;
                miniMapCamera.clearFlags = CameraClearFlags.Skybox;
                if (miniMapShader != null) {
                    miniMapCamera.RenderWithShader(miniMapShader, "");
                } else {
                    miniMapCamera.Render();
                }
            }

            if (renderOverlay == true) {
                miniMapCamera.cullingMask = overlayMask;
                miniMapCamera.clearFlags = CameraClearFlags.Nothing;
                if (overlayShader != null) {
                    miniMapCamera.RenderWithShader(overlayShader, "");
                } else {
                    miniMapCamera.Render();
                }
            }
        }

    }

}
