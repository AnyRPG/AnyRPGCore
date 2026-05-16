using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class UnitFrameProps {

        [Header("UNIT FRAME SETTINGS")]

        [Tooltip("If true, a snapshot of the target will be used.  If False, the UnitProfile image will be used.")]
        [SerializeField]
        private bool useSnapShot = true;

        [Tooltip("An object or bone in the heirarchy to use as the camera target.")]
        [SerializeField]
        private string unitFrameTarget = string.Empty;

        [Tooltip("The position the camera is looking at, relative to the target")]
        [SerializeField]
        private Vector3 unitFrameCameraLookOffset = Vector3.zero;

        [Tooltip("The position of the camera relative to the target")]
        [SerializeField]
        private Vector3 unitFrameCameraPositionOffset = new Vector3(0f, 0f, 0.66f);

        public string UnitFrameTarget { get => unitFrameTarget; set => unitFrameTarget = value; }
        public Vector3 UnitFrameCameraLookOffset { get => unitFrameCameraLookOffset; set => unitFrameCameraLookOffset = value; }
        public Vector3 UnitFrameCameraPositionOffset { get => unitFrameCameraPositionOffset; set => unitFrameCameraPositionOffset = value; }
        public bool UseSnapShot { get => useSnapShot; set => useSnapShot = value; }
    }


}