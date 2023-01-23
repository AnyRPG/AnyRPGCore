using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MusicPlayerManager : InteractableOptionManager {

        private MusicPlayerProps musicPlayerProps = null;

        public MusicPlayerProps MusicPlayerProps { get => musicPlayerProps; set => musicPlayerProps = value; }

        public void SetMusicPlayerProps(MusicPlayerProps musicPlayerProps, InteractableOptionComponent interactableOptionComponent) {
            this.musicPlayerProps = musicPlayerProps;

            BeginInteraction(interactableOptionComponent);
        }

        public override void EndInteraction() {
            base.EndInteraction();

            musicPlayerProps = null;
        }

    }

}