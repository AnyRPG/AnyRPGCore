using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    [System.Serializable]
    public class UINavigationController : ConfiguredClass {

        [SerializeField]
        private bool closeable = true;

        [SerializeField]
        private NavigationControllerDirection navigationDirection = NavigationControllerDirection.Vertical;

        [SerializeField]
        private List<Button> navigableButtons = new List<Button>();

        // setting index to -1 so that if gamepad isn't default, the first down press will highlight the first button instead of the second one
        private int currentIndex = -1;

        private ICloseableWindowContents owner = null;

        public bool Closeable { get => closeable; }
        public NavigationControllerDirection NavigationDirection { get => navigationDirection; }

        private ControlsManager controlsManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            controlsManager = systemGameManager.ControlsManager;
        }

        public void SetOwner(ICloseableWindowContents closeableWindowContents) {
            owner = closeableWindowContents;
        }

        public void RegisterNavigationController() {
            Debug.Log("UINavigationController.RegisterNavigationController()");
            controlsManager.AddNavigationController(this);
        }

        public void UnRegisterNavigationController() {
            Debug.Log("UINavigationController.UnRegisterNavigationController()");
            controlsManager.RemoveNavigationController(this);
        }

        public void FocusInitialButton() {
            Debug.Log("UINavigationController.FocusInitialButton()");
            if (navigableButtons.Count == 0) {
                return;
            }
            //navigableButtons[currentIndex].Select();
            EventSystem.current.SetSelectedGameObject(navigableButtons[currentIndex].gameObject);
        }

        public void NextButton() {
            Debug.Log("UINavigationController.NextButton()");
            if (navigableButtons.Count == 0) {
                return;
            }
            currentIndex++;
            if (currentIndex >= navigableButtons.Count) {
                currentIndex = 0;
            }
            EventSystem.current.SetSelectedGameObject(navigableButtons[currentIndex].gameObject);
            //navigableButtons[currentIndex].Select();
        }

        public void PreviousButton() {
            Debug.Log("UINavigationController.PreviousButton()");
            if (navigableButtons.Count == 0) {
                return;
            }
            currentIndex--;
            if (currentIndex < 0) {
                currentIndex = navigableButtons.Count - 1;
            }
            navigableButtons[currentIndex].Select();
        }

        public void Cancel() {
            if (closeable == true) {
                owner.Close();
            }
        }

        public void Accept() {
            if (navigableButtons.Count == 0) {
                return;
            }
            navigableButtons[currentIndex].onClick.Invoke();
        }

    }

    public enum NavigationControllerDirection { Vertical, Horizontal }
}

