using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class MusicPlayerProps : InteractableOptionProps {

        [Header("Music Player")]

        [Tooltip("The names of the audio profiles available on this music player")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private List<string> musicProfileNames = new List<string>();

        private List<AudioProfile> musicProfileList = new List<AudioProfile>();

        public override Sprite Icon { get => (SystemGameManager.Instance.SystemConfigurationManager.MusicPlayerInteractionPanelImage != null ? SystemGameManager.Instance.SystemConfigurationManager.MusicPlayerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemGameManager.Instance.SystemConfigurationManager.MusicPlayerNamePlateImage != null ? SystemGameManager.Instance.SystemConfigurationManager.MusicPlayerNamePlateImage : base.NamePlateImage); }
        public List<AudioProfile> MusicProfileList { get => musicProfileList; set => musicProfileList = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new MusicPlayerComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (musicProfileNames != null) {
                foreach (string musicProfileName in musicProfileNames) {
                    AudioProfile tmpMusicProfile = SystemAudioProfileManager.Instance.GetResource(musicProfileName);
                    if (tmpMusicProfile != null) {
                        musicProfileList.Add(tmpMusicProfile);
                    } else {
                        Debug.LogError("MusicPlayerCompoennt.SetupScriptableObjects(): COULD NOT FIND AUDIO PROFILE: " + musicProfileName + " while initializing");
                    }
                }
            }

        }
    }

}