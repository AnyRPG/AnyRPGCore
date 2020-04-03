using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CameraShader : MonoBehaviour {

        [SerializeField]
        private Shader miniMapShader = null;

        [SerializeField]
        private Shader overlayShader = null;

        [SerializeField]
        private bool limitFrameRates = false;

        [SerializeField]
        private int targetFrameRate = 10;

        private float accumulatedTime = 0;
        private float frameLength = 0f;

        private Camera miniMapCamera;

        private int defaultLayer;
        private int walkableLayer;
        private int waterLayer;
        private int minimapLayer;

        // Start is called before the first frame update
        void Start() {
            miniMapCamera = GetComponent<Camera>();
            miniMapCamera.enabled = false;

            defaultLayer = LayerMask.NameToLayer("Default");
            walkableLayer = LayerMask.NameToLayer("Walkable");
            waterLayer = LayerMask.NameToLayer("Water");
            minimapLayer = LayerMask.NameToLayer("MiniMap");

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
            if (miniMapShader != null) {
                miniMapCamera.cullingMask = ((1 << defaultLayer) | (1 << walkableLayer) | (1 << waterLayer));
                miniMapCamera.clearFlags = CameraClearFlags.Skybox;
                miniMapCamera.RenderWithShader(miniMapShader, "");
            }
            if (overlayShader != null) {
                miniMapCamera.cullingMask = (1 << minimapLayer);
                miniMapCamera.clearFlags = CameraClearFlags.Nothing;
                miniMapCamera.RenderWithShader(overlayShader, "");
            }
        }
    }

}
