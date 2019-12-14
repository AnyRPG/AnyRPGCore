using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class MusicPlayerHighlightButton : HighlightButton {

        private MusicProfile musicProfile;

        public MusicProfile MyMusicProfile { get => musicProfile; }

        public void SetMusicProfile(MusicProfile newMusicProfile) {
            if (newMusicProfile != null) {
                musicProfile = newMusicProfile;
            }
        }

        public override void Select() {
            //Debug.Log(gameObject.name + ".MusicPlayerHighlightButton.Select()");

            base.Select();
            MusicPlayerUI.MyInstance.MySelectedMusicPlayerHighlightButton = this;

            //GetComponent<Text>().color = Color.red;
            MusicPlayerUI.MyInstance.ShowDescription(musicProfile);

        }

        public override void DeSelect() {
            //Debug.Log("MusicPlayerHighlightButton.Deselect()");

            base.DeSelect();
        }

    }

}