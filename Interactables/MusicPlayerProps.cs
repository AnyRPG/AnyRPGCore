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
        private List<string> musicProfileNames = new List<string>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyMusicPlayerInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyMusicPlayerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyMusicPlayerNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyMusicPlayerNamePlateImage : base.NamePlateImage); }

        public InteractableOption GetInteractableOption(Interactable interactable) {
            return new MusicPlayer(interactable, this);
        }
    }

}