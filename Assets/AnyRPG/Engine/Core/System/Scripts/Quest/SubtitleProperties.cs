using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [System.Serializable]
    public class SubtitleProperties : ConfiguredClass {

        [Tooltip("The name of an audio profile to play when this dialog is started.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string audioProfileName = string.Empty;

        private AudioProfile audioProfile;

        [SerializeField]
        private List<SubtitleNode> subtitleNodes = new List<SubtitleNode>();

        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public List<SubtitleNode> SubtitleNodes { get => subtitleNodes; set => subtitleNodes = value; }
        public AudioProfile AudioProfile { get => audioProfile; set => audioProfile = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            if (audioProfileName != null && audioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = systemDataFactory.GetResource<AudioProfile>(audioProfileName);
                if (tmpAudioProfile != null) {
                    audioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("DialogProperties.SetupScriptableObjects(): Could not find audioProfile " + audioProfileName + " while initializing");
                }
            }
        }

    }
}