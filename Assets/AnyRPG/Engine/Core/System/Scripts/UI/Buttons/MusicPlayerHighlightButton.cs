using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class MusicPlayerHighlightButton : HighlightButton {

        private AudioProfile musicProfile;

        public AudioProfile MyMusicProfile { get => musicProfile; }

        public void SetMusicProfile(AudioProfile newMusicProfile) {
            if (newMusicProfile != null) {
                musicProfile = newMusicProfile;
            }
        }

        public override void Select() {
            //Debug.Log(gameObject.name + ".MusicPlayerHighlightButton.Select()");

            base.Select();
            MusicPlayerUI.Instance.MySelectedMusicPlayerHighlightButton = this;

            MusicPlayerUI.Instance.ShowDescription(musicProfile);

        }

        public override void DeSelect() {
            //Debug.Log("MusicPlayerHighlightButton.Deselect()");

            base.DeSelect();
        }

    }

}