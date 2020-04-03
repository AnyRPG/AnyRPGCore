using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class TextLogController : MonoBehaviour {

        [SerializeField]
        private Text text = null;

        //[SerializeField]
        //private int defaultFontSize = 18;

        //private string displayText;
        //private Color textColor;

        /*
        void Start() {
            
        }
        */

        public void InitializeTextLogController(string textToDisplay) {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            //text.color = textColor;
            text.text = textToDisplay;
        }

    }

}