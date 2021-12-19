using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class HintPanelController : WindowContentController {

        [Header("Hint Panel")]

        [SerializeField]
        private HighlightButton previousButton = null;

        [SerializeField]
        private HighlightButton nextButton = null;

        [SerializeField]
        private HighlightButton closeButton = null;

        [SerializeField]
        private Image hintImage = null;

        [SerializeField]
        private List<Sprite> hintImages = new List<Sprite>();

        private int hintIndex = 0;

        // game manager references
        protected UIManager uIManager = null;


        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        /*
        public void ClearSettings() {
            hintIndex = 0;
        }
        */

        /*
        public void Close() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.dialogWindow.CloseWindow();
        }
        */

        public void Previous() {
            hintIndex--;
            ShowCurrentHint();
        }

        public void Next() {
            //Debug.Log("NewGameMenuController.ConfirmAction(): dialogIndex: " + dialogIndex + "; DialogNode Count: " + MyDialog.MyDialogNodes.Count);
            hintIndex++;
            ShowCurrentHint();
        }

        public void ShowCurrentHint() {
            hintImage.sprite = hintImages[hintIndex];
            if (hintIndex == 0) {
                previousButton.Button.interactable = false;
            } else {
                previousButton.Button.interactable = true;
            }
            if (hintIndex >= (hintImages.Count - 1)) {
                nextButton.gameObject.SetActive(false);
                closeButton.gameObject.SetActive(true);
            } else {
                nextButton.gameObject.SetActive(true);
                closeButton.gameObject.SetActive(false);
            }

            uINavigationControllers[0].UpdateNavigationList();

            // on last page, close button should always be highlighted
            if (hintIndex >= (hintImages.Count - 1)) {
                uINavigationControllers[0].SetCurrentButton(closeButton);
            }

            uINavigationControllers[0].FocusCurrentButton();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("DialogPanelController.OnOpenWindow()");
            base.ProcessOpenWindowNotification();

            // these go first or they will squish the text of the continue button out of place
            hintIndex = 0;
            previousButton.gameObject.SetActive(true);

            ShowCurrentHint();
            PlayerPrefs.SetInt("ShowNewPlayerHints", 0);

            SetNavigationController(uINavigationControllers[0]);
        }


    }

}