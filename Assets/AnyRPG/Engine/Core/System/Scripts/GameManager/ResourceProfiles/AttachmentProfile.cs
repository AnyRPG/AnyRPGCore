using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Attachment Profile", menuName = "AnyRPG/AttachmentProfile")]
    public class AttachmentProfile : DescribableResource {

        [Header("Attachment Points")]

        [Tooltip("If this prefab is a unit, these nodes can be referenced by equipment when searching for attachment points")]
        [SerializeField]
        private List<AttachmentPointNode> attachmentPointNodes = new List<AttachmentPointNode>();

        private Dictionary<string, AttachmentPointNode> attachmentPointDictionary = new Dictionary<string, AttachmentPointNode>();

        public Dictionary<string, AttachmentPointNode> AttachmentPointDictionary { get => attachmentPointDictionary; set => attachmentPointDictionary = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            // define attachment point node dictionary for quick lookup later
            if (attachmentPointNodes != null) {
                foreach (AttachmentPointNode attachmentPointNode in attachmentPointNodes) {
                    if (attachmentPointNode != null && attachmentPointNode.NodeName != null && attachmentPointNode.NodeName != string.Empty) {
                        attachmentPointDictionary.Add(attachmentPointNode.NodeName, attachmentPointNode);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class AttachmentPointNode {

        [Tooltip("The name this node will be referrered to by when equipment is searching for it")]
        [SerializeField]
        private string nodeName = string.Empty;

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

        [Tooltip("The name of the audio profile to play when moving the prefab to this position")]
        [SerializeField]
        private string audioProfileName = string.Empty;

        private AudioProfile audioProfile = null;

        public Vector3 Position { get => position; set => position = value; }
        public bool RotationIsGlobal { get => rotationIsGlobal; set => rotationIsGlobal = value; }
        public Vector3 Rotation { get => rotation; set => rotation = value; }
        public Vector3 Scale { get => scale; set => scale = value; }
        public string TargetBone { get => targetBone; set => targetBone = value; }
        public string AudioProfileName { get => audioProfileName; set => audioProfileName = value; }
        public string NodeName { get => nodeName; set => nodeName = value; }

        public void SetupScriptableObjects() {
            if (audioProfileName != null && audioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = SystemDataFactory.Instance.GetResource<AudioProfile>(audioProfileName);
                if (tmpAudioProfile != null) {
                    audioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("PrefabProfile.SetupScriptableObjects():UNABLE TO FIND AudioProfile " + audioProfileName + " while initializing an attachment point node. CHECK INSPECTOR!");
                }
            }
        }

    }
}