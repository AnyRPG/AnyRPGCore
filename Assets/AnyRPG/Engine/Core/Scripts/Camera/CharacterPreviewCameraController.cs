using AnyRPG;
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