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

        [SerializeField]
        private GameObject aOption = null;

        [SerializeField]
        private GameObject aImage = null;

        [SerializeField]
        private GameObject xOption = null;

        [SerializeField]
        private GameObject xImage = null;

        [SerializeField]
        private GameObject yOption = null;

        [SerializeField]
        private GameObject yImage = null;

        [SerializeField]
        private GameObject bOption = null;

        [SerializeField]
        private GameObject bImage = null;

        [SerializeField]
        private TMP_Text aOptionText = null;

        [SerializeField]
        private TMP_Text xOptionText = null;

        [SerializeField]
        private TMP_Text yOptionText = null;

        [SerializeField]
        private TMP_Text bOptionText = null;

        public void Show() {
            gameObject.SetActive(true);
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void SetOptions(string aOptionString, string xOptionString, string yOptionString, string bOptionString) {
            Debug.Log("HintBarController.SetOptions()");
            if (aOptionString != null && aOptionString != string.Empty) {
                //aOption.SetActive(true);
                aImage.SetActive(true);
                aOptionText.gameObject.SetActive(true);
                aOptionText.text = aOptionString;
            } else {
                //aOption.SetActive(false);
                aImage.SetActive(false);
                aOptionText.gameObject.SetActive(false);
            }
            if (xOptionString != null && xOptionString != string.Empty) {
                //xOption.SetActive(true);
                xImage.SetActive(true);
                xOptionText.gameObject.SetActive(true);
                xOptionText.text = xOptionString;
            } else {
                //xOption.SetActive(false);
                xImage.SetActive(false);
                xOptionText.gameObject.SetActive(false);
            }
            if (yOptionString != null && yOptionString != string.Empty) {
                //yOption.SetActive(true);
                yImage.SetActive(true);
                yOptionText.gameObject.SetActive(true);
                yOptionText.text = yOptionString;
            } else {
                //yOption.SetActive(false);
                yImage.SetActive(false);
                yOptionText.gameObject.SetActive(false);
            }
            if (bOptionString != null && bOptionString != string.Empty) {
                //bOption.SetActive(true);
                bImage.SetActive(true);
                bOptionText.gameObject.SetActive(true);
                bOptionText.text = bOptionString;
            } else {
                //bOption.SetActive(false);
                bImage.SetActive(false);
                bOptionText.gameObject.SetActive(false);
            }
            Show();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

    }

}
