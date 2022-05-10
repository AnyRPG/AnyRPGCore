using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class MusicPlayerProps : InteractableOptionProps {

        [Header("Sound Player")]

        [Tooltip("The type of sound to play.  This affects which audio source will be used")]
        [SerializeField]
        private AudioType audioType = AudioType.Music;

        [Tooltip("The names of the audio profiles available on this music player")]
        [FormerlySerializedAs("musicProfileNames")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private List<string> audioProfileNames = new List<string>();

        private List<AudioProfile> audioProfileList = new List<AudioProfile>();

        public override Sprite Icon { get => (systemConfigurationManager.MusicPlayerInteractionPanelImage != null ? systemConfigurationManager.MusicPlayerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.MusicPlayerNamePlateImage != null ? systemConfigurationManager.MusicPlayerNamePlateImage : base.NamePlateImage); }
        public List<AudioProfile> AudioProfileList { get => audioProfileList; set => audioProfileList = value; }
        public AudioType AudioType { get => audioType; set => audioType = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new MusicPlayerComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            if (audioProfileNames != null) {
                foreach (string audioProfileName in audioProfileNames) {
                    AudioProfile tmpAudioProfile = systemDataFactory.GetResource<AudioProfile>(audioProfileName);
                    if (tmpAudioProfile != null) {
                        audioProfileList.Add(tmpAudioProfile);
                    } else {
                        Debug.LogError("MusicPlayerComponent.SetupScriptableObjects(): COULD NOT FIND AUDIO PROFILE: " + audioProfileName + " while initializing");
                    }
                }
            }

        }
    }

}