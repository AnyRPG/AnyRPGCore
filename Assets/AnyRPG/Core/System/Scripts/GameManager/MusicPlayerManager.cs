using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MusicPlayerManager : ConfiguredMonoBehaviour {

        public event System.Action OnConfirmAction = delegate { };
        public event System.Action OnEndInteraction = delegate { };

        private MusicPlayerProps musicPlayerProps = null;

        // game manager references
        private PlayerManager playerManager = null;

        public MusicPlayerProps MusicPlayerProps { get => musicPlayerProps; set => musicPlayerProps = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void SetMusicPlayerProps(MusicPlayerProps musicPlayerProps) {
            this.musicPlayerProps = musicPlayerProps;
        }

        public void PlayMusic() {

            OnConfirmAction();
        }

        public void EndInteraction() {
            OnEndInteraction();
        }



    }

}