using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class TextLogController : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI text = null;

        public void InitializeTextLogController(string textToDisplay) {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            text.text = textToDisplay;
        }

    }

}