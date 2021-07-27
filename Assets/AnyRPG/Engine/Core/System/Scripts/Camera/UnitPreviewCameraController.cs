using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class UnitPreviewCameraController : PreviewCameraController {

        //public event System.Action OnTargetReady = delegate { };

        protected override void Awake() {
            base.Awake();
            currentCamera = CameraManager.Instance.UnitPreviewCamera;
        }

    }

}