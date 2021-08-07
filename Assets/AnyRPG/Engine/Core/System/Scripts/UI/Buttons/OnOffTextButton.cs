using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class OnOffTextButton : ConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI text = null;

        //[SerializeField]
        //private GameObject hightlightImage = null;

        //[SerializeField]
        //private bool useHighlightColor = false;

        [SerializeField]
        private Color highlightColor = Color.blue;

        [SerializeField]
        private Color baseColor = Color.gray;

        public TextMeshProUGUI MyText { get => text; }

        // game manager references
        private AudioManager audioManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            audioManager = systemGameManager.AudioManager;
        }

        public void Select() {
            //Debug.Log(gameObject.name + ".HightlightButton.Select()");
            text.text = "on";
        }

        public void DeSelect() {
            //Debug.Log(gameObject.name + ".HightlightButton.DeSelect()");
            text.text = "off";
        }

        public virtual void OnHoverSound() {
            audioManager.PlayUIHoverSound();
        }

        public virtual void OnClickSound() {
            audioManager.PlayUIClickSound();
        }

    }

}