using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class NamePlateProps {

        [Header("NAMEPLATE SETTINGS")]

        [Tooltip("This is what will be printed on the nameplate above the object.  It will also override whatever value is set for the Interactable mouseover display name.")]
        [SerializeField]
        private string displayName = string.Empty;

        [Tooltip("If true, the nameplate is not shown above this unit.")]
        [SerializeField]
        private bool suppressNamePlate = false;

        [Tooltip("If true, the faction will not be shown on the nameplate")]
        [SerializeField]
        private bool suppressFaction = true;

        [Tooltip("If true, the nameplate position will be set to this value")]
        [SerializeField]
        private bool overrideNameplatePosition = false;

        [Tooltip("The position of the NamePlate anchor, relative to the unit pivot")]
        [SerializeField]
        private Vector3 namePlatePosition = Vector3.zero;

        [Header("UNIT FRAME SETTINGS")]

        [Tooltip("An object or bone in the heirarchy to use as the camera target.")]
        [SerializeField]
        private string unitFrameTarget = string.Empty;

        [Tooltip("The position the camera is looking at, relative to the target")]
        [SerializeField]
        private Vector3 unitFrameCameraLookOffset = Vector3.zero;

        [Tooltip("The position of the camera relative to the target")]
        [SerializeField]
        private Vector3 unitFrameCameraPositionOffset = new Vector3(0f, 0f, 0.66f);

        [Header("UNIT PREVIEW SETTINGS")]

        [Tooltip("a string that represents the name of the transform in the heirarchy that we will attach the camera to when this character is displayed in a player preview type of window")]
        [SerializeField]
        private string unitPreviewTarget = string.Empty;

        [SerializeField]
        private Vector3 unitPreviewCameraLookOffset = new Vector3(0f, 1f, 0f);

        [SerializeField]
        private Vector3 unitPreviewCameraPositionOffset = new Vector3(0f, 1f, 2.5f);

        public string DisplayName { get => displayName; set => displayName = value; }
        public bool SuppressNamePlate { get => suppressNamePlate; set => suppressNamePlate = value; }
        public bool SuppressFaction { get => suppressFaction; set => suppressFaction = value; }
        public bool OverrideNameplatePosition { get => overrideNameplatePosition; set => overrideNameplatePosition = value; }
        public Vector3 NameplatePosition { get => namePlatePosition; set => namePlatePosition = value; }
        public string UnitFrameTarget { get => unitFrameTarget; set => unitFrameTarget = value; }
        public Vector3 UnitFrameCameraLookOffset { get => unitFrameCameraLookOffset; set => unitFrameCameraLookOffset = value; }
        public Vector3 UnitFrameCameraPositionOffset { get => unitFrameCameraPositionOffset; set => unitFrameCameraPositionOffset = value; }
        public string UnitPreviewTarget { get => unitPreviewTarget; set => unitPreviewTarget = value; }
        public Vector3 UnitPreviewCameraLookOffset { get => unitPreviewCameraLookOffset; set => unitPreviewCameraLookOffset = value; }
        public Vector3 UnitPreviewCameraPositionOffset { get => unitPreviewCameraPositionOffset; set => unitPreviewCameraPositionOffset = value; }
    }


}