using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Unit Prefab Profile", menuName = "AnyRPG/UnitPrefabProfile")]
    public class UnitPrefabProfile : DescribableResource {

        [Header("Prefab")]

        [Tooltip("The prefab to use for the base unit")]
        [SerializeField]
        private GameObject unitPrefab = null;

        [Tooltip("The prefab to use for the model, if the unit prefab doesn't already have a model.  UMA units currently don't require this.")]
        [SerializeField]
        private GameObject modelPrefab = null;

        [Header("Animation")]

        [SerializeField]
        protected AnimationProfile animationProfile = null;

        [Tooltip("Should the model be rotated in the direction of travel while moving?  True is the best setting if no strafe or backing up animations exist on the animation profile.")]
        [SerializeField]
        protected bool rotateModel = false;

        [Header("UNIT FRAME")]

        [Tooltip("a string that represents the name of the transform in the heirarchy that we will attach the portrait camera to when this character is displayed in a unit frame")]
        [SerializeField]
        private string unitFrameTarget = string.Empty;

        [SerializeField]
        private Vector3 unitFrameCameraLookOffset = Vector3.zero;

        [SerializeField]
        private Vector3 unitFrameCameraPositionOffset = Vector3.zero;

        [Header("PLAYER PREVIEW")]

        [Tooltip("a string that represents the name of the transform in the heirarchy that we will attach the camera to when this character is displayed in a player preview type of window")]
        [SerializeField]
        private string playerPreviewTarget = string.Empty;

        [SerializeField]
        private Vector3 unitPreviewCameraLookOffset = new Vector3(0f, 1f, 0f);

        [SerializeField]
        private Vector3 unitPreviewCameraPositionOffset = new Vector3(0f, 1f, 1f);

        [Header("NAMEPLATE")]

        [Tooltip("If true, the nameplate is not shown above this unit.")]
        [SerializeField]
        private bool suppressNamePlate = false;

        [Tooltip("If true, the nameplate will not show the faction of the unit.")]
        [SerializeField]
        private bool suppressFaction = false;

        [Header("Mount")]

        [Tooltip("The transform position of the physical prefab in relation to the target bone")]
        [FormerlySerializedAs("physicalPosition")]
        [SerializeField]
        private Vector3 position = Vector3.zero;

        [Tooltip("should the rotation be local compared to it's parent or global (z forward, y up)")]
        [SerializeField]
        private bool rotationIsGlobal = false;

        [Tooltip("The transform rotation of the physical prefab")]
        [FormerlySerializedAs("physicalRotation")]
        [SerializeField]
        private Vector3 rotation = Vector3.zero;

        [Tooltip("The transform scale of the physical prefab")]
        [FormerlySerializedAs("physicalScale")]
        [SerializeField]
        private Vector3 scale = Vector3.one;

        [Tooltip("The bone on the character model to attach the physical prefab to")]
        [SerializeField]
        private string targetBone = string.Empty;

        [Header("Attachment Points")]

        [Tooltip("The name of the attachment profile to use for attaching other prefabs to this prefab")]
        [SerializeField]
        private string attachmentProfileName = string.Empty;

        // reference to the actual attachment profile
        private AttachmentProfile attachmentProfile = null;

        public Vector3 Position { get => position; }
        public Vector3 Rotation { get => rotation; }
        public Vector3 Scale { get => scale; }
        public string TargetBone { get => targetBone; }

        public bool RotationIsGlobal { get => rotationIsGlobal; set => rotationIsGlobal = value; }
        public AttachmentProfile AttachmentProfile { get => attachmentProfile; set => attachmentProfile = value; }
        public GameObject UnitPrefab { get => unitPrefab; set => unitPrefab = value; }
        public GameObject ModelPrefab { get => modelPrefab; set => modelPrefab = value; }
        public bool RotateModel { get => rotateModel; set => rotateModel = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (attachmentProfileName != null && attachmentProfileName != string.Empty) {
                AttachmentProfile tmpAttachmentProfile = SystemAttachmentProfileManager.MyInstance.GetResource(attachmentProfileName);
                if (tmpAttachmentProfile != null) {
                    attachmentProfile = tmpAttachmentProfile;
                } else {
                    Debug.LogError("PrefabProfile.SetupScriptableObjects(): UNABLE TO FIND AudioProfile " + attachmentProfileName + " while initializing " + DisplayName + ". CHECK INSPECTOR!");
                }
            }
        }
    }


}