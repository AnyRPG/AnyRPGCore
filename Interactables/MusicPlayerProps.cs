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

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MusicPlayerInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MusicPlayerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MusicPlayerNamePlateImage != null ? SystemConfigurationManager.MyInstance.MusicPlayerNamePlateImage : base.NamePlateImage); }
        public List<string> MusicProfileNames { get => musicProfileNames; set => musicProfileNames = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            return new MusicPlayerComponent(interactable, this);
        }
    }

}