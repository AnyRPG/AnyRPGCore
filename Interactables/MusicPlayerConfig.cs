using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Music Player Config", menuName = "AnyRPG/Interactable/MusicPlayerConfig")]
    public class MusicPlayerConfig : InteractableOptionConfig {

        [SerializeField]
        private MusicPlayerProps interactableOptionProps = new MusicPlayerProps();

        [Header("Music Player")]

        [Tooltip("The names of the audio profiles available on this music player")]
        [SerializeField]
        private List<string> musicProfileNames = new List<string>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyMusicPlayerInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyMusicPlayerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyMusicPlayerNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyMusicPlayerNamePlateImage : base.NamePlateImage); }
        public MusicPlayerProps InteractableOptionProps { get => interactableOptionProps; set => interactableOptionProps = value; }
    }

}