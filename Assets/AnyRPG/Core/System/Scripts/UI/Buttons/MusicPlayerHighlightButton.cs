using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class MusicPlayerHighlightButton : HighlightButton {

        protected MusicPlayerPanel musicPlayerUI = null;

        protected AudioProfile musicProfile = null;

        public AudioProfile MusicProfile { get => musicProfile; }

        public void SetMusicProfile(MusicPlayerPanel musicPlayerUI, AudioProfile newMusicProfile) {
            this.musicPlayerUI = musicPlayerUI;
            if (newMusicProfile != null) {
                musicProfile = newMusicProfile;
            }
        }

        public override void Select() {
            //Debug.Log($"{gameObject.name}.MusicPlayerHighlightButton.Select()");

            base.Select();
            musicPlayerUI.SetSelectedButton(this);

        }

    }

}