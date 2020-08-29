using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UMA;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Prefab Profile", menuName = "AnyRPG/PrefabProfile")]
    public class PrefabProfile : DescribableResource {

        [Header("Prefab")]

        /// <summary>
        /// The prefab object to attach to the character when equipping this item
        /// </summary>
        /// 
        [FormerlySerializedAs("physicalPrefab")]
        [SerializeField]
        private GameObject prefab = null;

        [Header("Held")]

        /// <summary>
        /// The transform position of the physical prefab in relation to the target bone
        /// </summary>
        [FormerlySerializedAs("physicalPosition")]
        [SerializeField]
        private Vector3 position = Vector3.zero;

        /// <summary>
        /// should the rotation be local compared to it's parent or global (z forward, y up)
        /// </summary>
        [SerializeField]
        private bool rotationIsGlobal = false;

        /// <summary>
        /// The transform rotation of the physical prefab
        /// </summary>
        [FormerlySerializedAs("physicalRotation")]
        [SerializeField]
        private Vector3 rotation = Vector3.zero;

        /// <summary>
        /// The transform scale of the physical prefab
        /// </summary>
        [FormerlySerializedAs("physicalScale")]
        [SerializeField]
        private Vector3 scale = Vector3.one;

        /// <summary>
        /// The bone on the character model to attach the physical prefab to
        /// </summary>
        [SerializeField]
        private string targetBone = string.Empty;

        [SerializeField]
        private string unsheathAudioProfileName = string.Empty;
        private AudioProfile unsheathAudioProfile = null;

        [Header("Sheathed")]

        [FormerlySerializedAs("sheathedPhysicalPosition")]
        [SerializeField]
        private Vector3 sheathedPosition = Vector3.zero;


        [FormerlySerializedAs("sheathedPhysicalRotation")]
        [SerializeField]
        private Vector3 sheathedRotation = Vector3.zero;


        [FormerlySerializedAs("sheathedPhysicalScale")]
        [SerializeField]
        private Vector3 sheathedScale = Vector3.one;


        [SerializeField]
        private string sheathedTargetBone = string.Empty;

        [SerializeField]
        private string sheathAudioProfileName = string.Empty;
        private AudioProfile sheathAudioProfile = null;

        [Header("Pickup")]

        [Tooltip("If true, this will be used for item pickups instead of sheathed")]
        [SerializeField]
        private bool useItemPickup = false;

        [FormerlySerializedAs("sheathedPhysicalPosition")]
        [SerializeField]
        private Vector3 pickupPosition = Vector3.zero;


        [FormerlySerializedAs("sheathedPhysicalRotation")]
        [SerializeField]
        private Vector3 pickupRotation = Vector3.zero;


        [FormerlySerializedAs("sheathedPhysicalScale")]
        [SerializeField]
        private Vector3 pickupScale = Vector3.one;


        public GameObject MyPrefab { get => prefab; }
        public Vector3 MyPosition { get => position; }
        public Vector3 MyRotation { get => rotation; }
        public Vector3 MyScale { get => scale; }
        public string TargetBone { get => targetBone; }

        public Vector3 SheathedPosition { get => sheathedPosition; }
        public Vector3 SheathedRotation { get => sheathedRotation; }
        public Vector3 SheathedScale { get => sheathedScale; }
        public string SheathedTargetBone { get => sheathedTargetBone; }
        public bool RotationIsGlobal { get => rotationIsGlobal; set => rotationIsGlobal = value; }
        public AudioProfile UnsheathAudioProfile { get => unsheathAudioProfile; set => unsheathAudioProfile = value; }
        public AudioProfile SheathAudioProfile { get => sheathAudioProfile; set => sheathAudioProfile = value; }
        public bool UseItemPickup { get => useItemPickup; set => useItemPickup = value; }
        public Vector3 PickupPosition { get => pickupPosition; set => pickupPosition = value; }
        public Vector3 PickupRotation { get => pickupRotation; set => pickupRotation = value; }
        public Vector3 PickupScale { get => pickupScale; set => pickupScale = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (sheathAudioProfileName != null && sheathAudioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = SystemAudioProfileManager.MyInstance.GetResource(sheathAudioProfileName);
                if (tmpAudioProfile != null) {
                    sheathAudioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("PrefabProfile.SetupScriptableObjects():UNABLE TO FIND AudioProfile " + sheathAudioProfile + " while initializing " + DisplayName + ". CHECK INSPECTOR!");
                }
            }

            if (unsheathAudioProfileName != null && unsheathAudioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = SystemAudioProfileManager.MyInstance.GetResource(unsheathAudioProfileName);
                if (tmpAudioProfile != null) {
                    unsheathAudioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("PrefabProfile.SetupScriptableObjects(): UNABLE TO FIND AudioProfile " + unsheathAudioProfile + " while initializing " + DisplayName + ". CHECK INSPECTOR!");
                }
            }
        }
    }
}