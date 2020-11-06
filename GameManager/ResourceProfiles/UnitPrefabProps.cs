using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class UnitPrefabProps {

        [Header("Prefab")]

        [Tooltip("The prefab to use for the base unit")]
        [SerializeField]
        private GameObject unitPrefab = null;

        [Tooltip("The prefab to use for the model, if the unit prefab doesn't already have a model.  UMA units currently don't require this.")]
        [SerializeField]
        private GameObject modelPrefab = null;

        [Header("Animation")]

        [Tooltip("A shared animation profile to be used for the unit animations")]
        [SerializeField]
        private AnimationProfile animationProfile = null;

        [Tooltip("If true, the inline values below will be used instead of the animation profile above")]
        [SerializeField]
        protected bool useInlineAnimationProps = false;

        [Tooltip("If useInlineAnimationProps is true, these values will be used instead of the shared animation profile above")]
        [SerializeField]
        private AnimationProps animationProps = new AnimationProps();

        [Tooltip("Should the model be rotated in the direction of travel while moving?  True is the best setting if no strafe or backing up animations exist on the animation profile.")]
        [SerializeField]
        protected bool rotateModel = false;

        [Header("NamePlate")]

        [SerializeField]
        protected NamePlateProps namePlateProps = new NamePlateProps();

        [Header("Mount")]

        [Tooltip("The transform position of the physical prefab in relation to the target bone")]
        [SerializeField]
        private Vector3 position = Vector3.zero;

        [Tooltip("should the rotation be local compared to it's parent or global (z forward, y up)")]
        [SerializeField]
        private bool rotationIsGlobal = false;

        [Tooltip("The transform rotation of the physical prefab")]
        [SerializeField]
        private Vector3 rotation = Vector3.zero;

        [Tooltip("The transform scale of the physical prefab")]
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
        public AnimationProfile AnimationProfile { get => animationProfile; set => animationProfile = value; }
        public NamePlateProps NamePlateProps { get => namePlateProps; set => namePlateProps = value; }

        public void SetupScriptableObjects() {

            if (attachmentProfileName != null && attachmentProfileName != string.Empty) {
                AttachmentProfile tmpAttachmentProfile = SystemAttachmentProfileManager.MyInstance.GetResource(attachmentProfileName);
                if (tmpAttachmentProfile != null) {
                    attachmentProfile = tmpAttachmentProfile;
                } else {
                    Debug.LogError("UnitPrefabProps.SetupScriptableObjects(): UNABLE TO FIND AudioProfile " + attachmentProfileName + " while initializing. CHECK INSPECTOR!");
                }
            }
        }
    }


}