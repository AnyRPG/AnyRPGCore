using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class UnitPrefabProps : ConfiguredClass {

        [Header("Prefab")]

        [Tooltip("The prefab to use for the base unit")]
        [SerializeField]
        private GameObject unitPrefab = null;

        [Tooltip("The prefab to use for the model, if the unit prefab doesn't already have a model.")]
        [SerializeField]
        private GameObject modelPrefab = null;

        [Tooltip("The type of character model for the purpose of character customization")]
        [SerializeReference]
        [SerializeReferenceButton]
        private CharacterModelProvider modelProvider = null;

        [Header("Animation")]

        [Tooltip("A shared animation profile to be used for the unit animations")]
        [ResourceSelector(resourceType = typeof(AnimationProfile))]
        [SerializeField]
        private string animationProfileName = null;

        /*
        [Tooltip("A shared animation profile to be used for the unit animations")]
        [SerializeField]
        */
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
        [ResourceSelector(resourceType = typeof(AttachmentProfile))]
        private string attachmentProfileName = string.Empty;

        [Header("Water Configuration")]

        [Tooltip("When floating in water the height from the bottom of the transform where the water line will come up to")]
        [SerializeField]
        private float floatHeight = 1.4f;

        [Tooltip("If true, height of the transform will be added to the value above. If false, transform height will replace float height if the transform is found")]
        [SerializeField]
        private bool addFloatHeightToTransform = false;

        [Tooltip("When floating in water, the child transform that will be used for the water line")]
        [FormerlySerializedAs("chestBone")]
        [SerializeField]
        private string floatTransform = "Spine1";

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
        public NamePlateProps NamePlateProps { get => namePlateProps; set => namePlateProps = value; }
        public AnimationProps AnimationProps {
            get {
                if (useInlineAnimationProps) {
                    return animationProps;
                }
                if (animationProfile != null) {
                    return animationProfile.AnimationProps;
                }
                return null;
            }
            set {
                animationProps = value;
            }
        }

        public string FloatTransform { get => floatTransform; set => floatTransform = value; }
        public float FloatHeight { get => floatHeight; }
        public bool AddFloatHeightToTransform { get => addFloatHeightToTransform; }
        public bool UseInlineAnimationProps { get => useInlineAnimationProps; set => useInlineAnimationProps = value; }
        public string AttachmentProfileName { get => attachmentProfileName; set => attachmentProfileName = value; }
        public CharacterModelProvider ModelProvider { get => modelProvider; set => modelProvider = value; }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {

            Configure(systemGameManager);

            animationProps.Configure();

            if (animationProfileName != null && animationProfileName != string.Empty) {
                AnimationProfile tmpAnimationProfile = systemDataFactory.GetResource<AnimationProfile>(animationProfileName);
                if (tmpAnimationProfile != null) {
                    animationProfile = tmpAnimationProfile;
                } else {
                    Debug.LogError("UnitPrefabProps.SetupScriptableObjects(): UNABLE TO FIND Animation Profile " + animationProfileName + " while initializing. CHECK INSPECTOR!");
                }
            }


            if (attachmentProfileName != null && attachmentProfileName != string.Empty) {
                AttachmentProfile tmpAttachmentProfile = systemDataFactory.GetResource<AttachmentProfile>(attachmentProfileName);
                if (tmpAttachmentProfile != null) {
                    attachmentProfile = tmpAttachmentProfile;
                } else {
                    Debug.LogError("UnitPrefabProps.SetupScriptableObjects(): UNABLE TO FIND Attachment Profile " + attachmentProfileName + " while initializing. CHECK INSPECTOR!");
                }
            }

            if (modelProvider != null) {
                modelProvider.Configure(systemGameManager);
            }
        }
    }


}