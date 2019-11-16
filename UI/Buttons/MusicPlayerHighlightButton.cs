using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class MusicPlayerHighlightButton : HighlightButton {

        private string musicProfileName;

        public string MyMusicProfileName { get => musicProfileName; }

        public void SetMusicProfileName(string musicProfileName) {
            if (musicProfileName != null && musicProfileName != string.Empty) {
                this.musicProfileName = musicProfileName;
            }
        }

        public override void Select() {
            Debug.Log(gameObject.name + ".MusicPlayerHighlightButton.Select()");

            base.Select();
            MusicPlayerUI.MyInstance.MySelectedMusicPlayerHighlightButton = this;

            //GetComponent<Text>().color = Color.red;
            MusicPlayerUI.MyInstance.ShowDescription(musicProfileName);

        }

        public override void DeSelect() {
            Debug.Log("MusicPlayerHighlightButton.Deselect()");

            base.DeSelect();
        }

    }

}