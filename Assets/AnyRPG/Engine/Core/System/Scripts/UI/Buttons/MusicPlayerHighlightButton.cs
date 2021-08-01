using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class MusicPlayerHighlightButton : HighlightButton {

        private MusicPlayerUI musicPlayerUI = null;

        private AudioProfile musicProfile;

        public AudioProfile MyMusicProfile { get => musicProfile; }

        public void SetMusicProfile(MusicPlayerUI musicPlayerUI, AudioProfile newMusicProfile) {
            this.musicPlayerUI = musicPlayerUI;
            if (newMusicProfile != null) {
                musicProfile = newMusicProfile;
            }
        }

        public override void Select() {
            //Debug.Log(gameObject.name + ".MusicPlayerHighlightButton.Select()");

            base.Select();
            musicPlayerUI.MySelectedMusicPlayerHighlightButton = this;

            musicPlayerUI.ShowDescription(musicProfile);

        }

        public override void DeSelect() {
            //Debug.Log("MusicPlayerHighlightButton.Deselect()");

            base.DeSelect();
        }

    }

}