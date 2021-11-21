using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class OnOffTextButton : HighlightButton {

        /*
        [SerializeField]
        private TextMeshProUGUI text = null;

        [SerializeField]
        protected Color highlightColor = Color.blue;

        [SerializeField]
        protected Color baseColor = Color.gray;

        public TextMeshProUGUI MyText { get => text; }
        */


        public void SetOn() {
            //Debug.Log(gameObject.name + ".OnOffTextButton.SetOn()");
            text.text = "on";
        }

        public void SetOff() {
            //Debug.Log(gameObject.name + ".OnOffTextButton.SetOff()");
            text.text = "off";
        }


    }

}