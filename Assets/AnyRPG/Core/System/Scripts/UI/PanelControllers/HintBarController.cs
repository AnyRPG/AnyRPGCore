using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class HintBarController : ConfiguredMonoBehaviour {

        [SerializeField]
        private RectTransform rectTransform = null;

        /*
        [SerializeField]
        private GameObject aOption = null;
        */

        [SerializeField]
        private GameObject aImage = null;

        /*
        [SerializeField]
        private GameObject xOption = null;
        */

        [SerializeField]
        private GameObject xImage = null;

        /*
        [SerializeField]
        private GameObject yOption = null;
        */

        [SerializeField]
        private GameObject yImage = null;

        /*
        [SerializeField]
        private GameObject bOption = null;
        */

        [SerializeField]
        private GameObject bImage = null;

        [SerializeField]
        private GameObject dPadImage = null;


        [SerializeField]
        private TMP_Text aOptionText = null;

        [SerializeField]
        private TMP_Text xOptionText = null;

        [SerializeField]
        private TMP_Text yOptionText = null;

        [SerializeField]
        private TMP_Text bOptionText = null;

        [SerializeField]
        private TMP_Text dPadOptionText = null;

        public void Show() {
            gameObject.SetActive(true);
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void SetOptions(string aOptionString, string xOptionString, string yOptionString, string bOptionString, string dPadOptionString = "") {
            //Debug.Log("HintBarController.SetOptions()");
            if (aOptionString != null && aOptionString != string.Empty) {
                //aOption.SetActive(true);
                aImage.SetActive(true);
                aOptionText.text = aOptionString;
                aOptionText.gameObject.SetActive(true);
            } else {
                //aOption.SetActive(false);
                aImage.SetActive(false);
                aOptionText.gameObject.SetActive(false);
            }
            if (xOptionString != null && xOptionString != string.Empty) {
                //xOption.SetActive(true);
                xImage.SetActive(true);
                xOptionText.text = xOptionString;
                xOptionText.gameObject.SetActive(true);
            } else {
                //xOption.SetActive(false);
                xImage.SetActive(false);
                xOptionText.gameObject.SetActive(false);
            }
            if (yOptionString != null && yOptionString != string.Empty) {
                //yOption.SetActive(true);
                yImage.SetActive(true);
                yOptionText.text = yOptionString;
                yOptionText.gameObject.SetActive(true);
            } else {
                //yOption.SetActive(false);
                yImage.SetActive(false);
                yOptionText.gameObject.SetActive(false);
            }
            if (bOptionString != null && bOptionString != string.Empty) {
                //bOption.SetActive(true);
                bImage.SetActive(true);
                bOptionText.text = bOptionString;
                bOptionText.gameObject.SetActive(true);
            } else {
                //bOption.SetActive(false);
                bImage.SetActive(false);
                bOptionText.gameObject.SetActive(false);
            }
            if (dPadOptionString != null && dPadOptionString != string.Empty) {
                //bOption.SetActive(true);
                dPadImage.SetActive(true);
                dPadOptionText.text = dPadOptionString;
                dPadOptionText.gameObject.SetActive(true);
            } else {
                //bOption.SetActive(false);
                dPadImage.SetActive(false);
                dPadOptionText.gameObject.SetActive(false);
            }
            Show();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

    }

}
