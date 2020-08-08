using AnyRPG;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;

namespace AnyRPG {

    public class CharacterPreviewCameraController : PreviewCameraController {

        //public event System.Action OnTargetReady = delegate { };

        protected override void Awake() {
            //Debug.Log("CharacterPreviewCameraController.Awake()");
            base.Awake();
            currentCamera = CameraManager.MyInstance.CharacterPreviewCamera;
        }

    }

}