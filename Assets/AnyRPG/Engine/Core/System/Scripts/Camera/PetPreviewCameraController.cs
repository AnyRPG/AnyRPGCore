using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class PetPreviewCameraController : PreviewCameraController {

        //public event System.Action OnTargetReady = delegate { };
        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            currentCamera = cameraManager.PetPreviewCamera;
        }

    }

}