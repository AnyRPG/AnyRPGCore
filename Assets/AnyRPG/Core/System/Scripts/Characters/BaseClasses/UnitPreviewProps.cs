using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class UnitPreviewProps {

        [Header("UNIT PREVIEW SETTINGS")]

        [Tooltip("a string that represents the name of the transform in the heirarchy that we will attach the camera to when this character is displayed in a player preview type of window")]
        [SerializeField]
        private string unitPreviewTarget = string.Empty;

        [SerializeField]
        private Vector3 unitPreviewCameraLookOffset = new Vector3(0f, 1f, 0f);

        [SerializeField]
        private Vector3 unitPreviewCameraPositionOffset = new Vector3(0f, 1f, 2.5f);

        public string UnitPreviewTarget { get => unitPreviewTarget; set => unitPreviewTarget = value; }
        public Vector3 UnitPreviewCameraLookOffset { get => unitPreviewCameraLookOffset; set => unitPreviewCameraLookOffset = value; }
        public Vector3 UnitPreviewCameraPositionOffset { get => unitPreviewCameraPositionOffset; set => unitPreviewCameraPositionOffset = value; }
    }


}