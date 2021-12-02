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
        private GameObject aImage = null;

        [SerializeField]
        private GameObject xImage = null;

        [SerializeField]
        private GameObject yImage = null;

        [SerializeField]
        private GameObject bImage = null;

        [SerializeField]
        private GameObject dPadImage = null;

        [SerializeField]
        private GameObject rDownImage = null;

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

        [SerializeField]
        private TMP_Text rDownOptionText = null;

        // game manager references
        protected ControlsManager controlsManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            controlsManager = systemGameManager.ControlsManager;
        }

        public void Show() {
            gameObject.SetActive(true);
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void SetOptions(string aOptionString, string xOptionString, string yOptionString, string bOptionString, string dPadOptionString, string rDownOptionString) {
            //Debug.Log("HintBarController.SetOptions()");

            if (controlsManager.GamePadModeActive == false) {
                return;
            }

            if (aOptionString != null && aOptionString != string.Empty) {
                aImage.SetActive(true);
                aOptionText.text = aOptionString;
                aOptionText.gameObject.SetActive(true);
            } else {
                aImage.SetActive(false);
                aOptionText.gameObject.SetActive(false);
            }
            if (xOptionString != null && xOptionString != string.Empty) {
                xImage.SetActive(true);
                xOptionText.text = xOptionString;
                xOptionText.gameObject.SetActive(true);
            } else {
                xImage.SetActive(false);
                xOptionText.gameObject.SetActive(false);
            }
            if (yOptionString != null && yOptionString != string.Empty) {
                yImage.SetActive(true);
                yOptionText.text = yOptionString;
                yOptionText.gameObject.SetActive(true);
            } else {
                yImage.SetActive(false);
                yOptionText.gameObject.SetActive(false);
            }
            if (bOptionString != null && bOptionString != string.Empty) {
                bImage.SetActive(true);
                bOptionText.text = bOptionString;
                bOptionText.gameObject.SetActive(true);
            } else {
                bImage.SetActive(false);
                bOptionText.gameObject.SetActive(false);
            }
            if (dPadOptionString != null && dPadOptionString != string.Empty) {
                dPadImage.SetActive(true);
                dPadOptionText.text = dPadOptionString;
                dPadOptionText.gameObject.SetActive(true);
            } else {
                dPadImage.SetActive(false);
                dPadOptionText.gameObject.SetActive(false);
            }
            if (rDownOptionString != null && rDownOptionString != string.Empty) {
                rDownImage.SetActive(true);
                rDownOptionText.text = rDownOptionString;
                rDownOptionText.gameObject.SetActive(true);
            } else {
                rDownImage.SetActive(false);
                rDownOptionText.gameObject.SetActive(false);
            }
            Show();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

    }

}
