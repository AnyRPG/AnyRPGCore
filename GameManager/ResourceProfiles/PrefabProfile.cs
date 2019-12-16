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

        /// <summary>
        /// The prefab object to attach to the character when equipping this item
        /// </summary>
        /// 
        [FormerlySerializedAs("physicalPrefab")]
        [SerializeField]
        private GameObject prefab;

        /// <summary>
        /// The transform position of the physical prefab in relation to the target bone
        /// </summary>
        [FormerlySerializedAs("physicalPosition")]
        [SerializeField]
        private Vector3 position = Vector3.zero;

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
        private string targetBone;


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
        private string sheathedTargetBone;

        public GameObject MyPrefab { get => prefab; }
        public Vector3 MyPosition { get => position; }
        public Vector3 MyRotation { get => rotation; }
        public Vector3 MyScale { get => scale; }
        public string MyTargetBone { get => targetBone; }

        public Vector3 MySheathedPosition { get => sheathedPosition; }
        public Vector3 MySheathedRotation { get => sheathedRotation; }
        public Vector3 MySheathedScale { get => sheathedScale; }
        public string MySheathedTargetBone { get => sheathedTargetBone; }

    }
}